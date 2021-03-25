using DeliCounter.Backend;
using LibGit2Sharp;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Credentials = Octokit.Credentials;
using Signature = LibGit2Sharp.Signature;

namespace DatabaseUpdater
{
    internal static class Program
    {
        private enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        private static void ConsoleLog(LogLevel level, string message)
        {
            Console.ForegroundColor = level switch
            {
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => Console.ForegroundColor
            };
            Console.WriteLine($"[{level.ToString(),7}] {message}");
        }

        private static readonly Dictionary<string, VersionFetcher> Checkers = new()
        {
            ["bonetome.com"] = new BoneTomeVersionFetcher()
            
        };

        private static readonly GitHubClient GitHubClient = new(new ProductHeaderValue("DeliCounter-Updater"));
        
        private static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            if (args.Length > 0) GitHubClient.Credentials = new Credentials(args[0]);
            Checkers["github.com"] = new GitHubVersionFetcher(GitHubClient);
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings {Converters = new List<JsonConverter> {new SemRangeConverter(), new SemVersionConverter()}};
            ModRepository repo = new("https://github.com/Deli-Collective/DeliCounter.Database/tree/main");

            int alreadyUpToDate = 0, updated = 0, error = 0;

            foreach (Mod mod in repo.Mods.Values)
            {
                string sourceWebsite = mod.Latest.SourceUrl.Split("/")[2];
                if (!Checkers.TryGetValue(sourceWebsite, out VersionFetcher checker))
                {
                    ConsoleLog(LogLevel.Error, $"{mod.Guid}: No version fetcher for {sourceWebsite}");
                    continue;
                }

                try
                {
                    FetchedVersion latest = checker.GetLatestVersion(mod.Latest).GetAwaiter().GetResult();

                    if (mod.LatestVersion >= latest.Version)
                    {
                        ConsoleLog(LogLevel.Info, $"{mod.Guid} is up to date");
                        alreadyUpToDate++;
                        continue;
                    }

                    // If this isn't a versioned mod or we're only bumping the patch version, don't keep the old one around
                    if (!checker.Versioned || (mod.LatestVersion.Major == latest.Version.Major && mod.LatestVersion.Minor == latest.Version.Minor))
                    {
                        File.Delete($"ModRepository/{mod.Category.Path}/{mod.Guid}/{mod.LatestVersion}.json");
                    }

                    // Update the mod file
                    mod.Latest.VersionNumber = latest.Version;
                    mod.Latest.DownloadUrl = latest.DownloadUrl;

                    // Write the stuff
                    File.WriteAllText($"ModRepository/{mod.Category.Path}/{mod.Guid}/{latest.Version}.json", JsonConvert.SerializeObject(mod.Latest, Formatting.Indented));

                    ConsoleLog(LogLevel.Info, $"{mod.Guid} was updated from {mod.LatestVersion} to {latest.Version}");
                    updated++;
                }
                catch (Exception e)
                {
                    ConsoleLog(LogLevel.Error, $"Error checking latest version for {mod.Guid}: {e.Message}");
                    error++;
                }
            }
            
            ConsoleLog(LogLevel.Info, $"Done fetching versions: {updated} updated, {error} errors, {alreadyUpToDate} already up to date.");

            try
            {
                Commands.Stage(repo.Repo, "*");
                User user = GitHubClient.User.Current().GetAwaiter().GetResult();
                Signature sig = new(user.Name, user.Email, DateTimeOffset.Now);
                repo.Repo.Commit($"Updated {updated} mods in database", sig, sig);


                PushOptions options = new()
                {
                    CredentialsProvider = (_, _, _) =>
                        new UsernamePasswordCredentials {Username = user.Login, Password = args[0]}
                };
                repo.Repo.Network.Push(repo.Repo.Head, options);
                
                ConsoleLog(LogLevel.Info, "Pushed changes");
            }
            catch (LibGit2SharpException e)
            {
                ConsoleLog(LogLevel.Error, "Couldn't push changes: " + e.Message);
            }
            
            sw.Stop();
            ConsoleLog(LogLevel.Info, $"Done in {sw.ElapsedMilliseconds / 1000d}s!");
        }
    }
}
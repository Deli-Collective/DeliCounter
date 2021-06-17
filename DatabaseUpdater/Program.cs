using DeliCounter.Backend;
using LibGit2Sharp;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Windows.System.Threading;
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

        private enum ModUpdateStatus
        {
            UpToDate,
            Updated,
            Error,
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

        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly Dictionary<string, VersionFetcher> Checkers = new()
        {
            ["bonetome.com"] = new BoneTomeVersionFetcher()
            
        };

        private static readonly GitHubClient GitHubClient = new(new ProductHeaderValue("DeliCounter-Updater"));

        private static Queue<Mod> _queuedMods;
        private static List<string> _ignoredMods;
        private static int _alreadyUpToDate = 0, _updated = 0, _error = 0;
        private static ModRepository _repo;
        
        private static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            if (args.Length > 0) GitHubClient.Credentials = new Credentials(args[0]);
            Checkers["github.com"] = new GitHubVersionFetcher(GitHubClient);
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings {Converters = new List<JsonConverter> {new SemRangeConverter(), new SemVersionConverter()}};
            _repo = new ModRepository("https://github.com/Deli-Collective/DeliCounter.Database/tree/main");
            _ignoredMods = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("ModRepository/ignore_updates.json"));
            
            _queuedMods = new Queue<Mod>(_repo.Mods.Values);

            // Make our threads and start them
            Thread[] threads = new Thread[8];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(ThreadKernel);
                threads[i].Start();
            }
            
            // Let them each finish
            foreach (Thread t in threads) t.Join();

            ConsoleLog(LogLevel.Info, $"Done fetching versions: {_updated} updated, {_error} errors, {_alreadyUpToDate} already up to date.");
            
            if (_updated > 0) GitCommit(args[0]);

            sw.Stop();
            ConsoleLog(LogLevel.Info, $"Done in {sw.ElapsedMilliseconds / 1000d}s!");
        }

        private static void ThreadKernel()
        {
            while (true)
            {
                Mod mod;
                lock (_queuedMods) mod = _queuedMods.Count > 0 ? _queuedMods.Dequeue() : null;
                if (mod is null) return;
                UpdateMod(mod);
            }
        }
        
        private static void UpdateMod(Mod mod)
        {
            if (_ignoredMods.Contains(mod.Guid))
            {
                ConsoleLog(LogLevel.Info, $"{mod.Guid} is ignored. Will not check for update.");
                return;
            }
            
            string sourceWebsite = mod.Latest.SourceUrl.Split("/")[2];
            if (!Checkers.TryGetValue(sourceWebsite, out VersionFetcher checker))
            {
                ConsoleLog(LogLevel.Error, $"{mod.Guid}: No version fetcher for {sourceWebsite}");
                _error++;
                return;
            }

            try
            {
                FetchedVersion latest = checker.GetLatestVersion(mod.Latest).GetAwaiter().GetResult();

                if (mod.LatestVersion >= latest.Version)
                {
                    ConsoleLog(LogLevel.Info, $"{mod.Guid} is up to date");
                    _alreadyUpToDate++;
                    return;
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
                File.WriteAllText($"ModRepository/{mod.Category.Path}/{mod.Guid}/{latest.Version}.json", JsonConvert.SerializeObject(mod.Latest, Formatting.Indented, jsonSettings));

                ConsoleLog(LogLevel.Info, $"{mod.Guid} was updated from {mod.LatestVersion} to {latest.Version}");
                _updated++;
            }
            catch (Exception e)
            {
                ConsoleLog(LogLevel.Error, $"Error checking latest version for {mod.Guid}: {e.Message}");
                _error++;
            }
        }

        private static void GitCommit(string token)
        {
            try
            {
                Commands.Stage(_repo.Repo, "**/*");
                User user = GitHubClient.User.Current().GetAwaiter().GetResult();
                Signature sig = new(user.Name, user.Email ?? "deli-employee@example.com", DateTimeOffset.Now);
                _repo.Repo.Commit($"Updated {_updated} mods in database", sig, sig);


                PushOptions options = new()
                {
                    CredentialsProvider = (_, _, _) =>
                        new UsernamePasswordCredentials {Username = user.Login, Password = token}
                };
                _repo.Repo.Network.Push(_repo.Repo.Head, options);
                
                ConsoleLog(LogLevel.Info, "Pushed changes");
            }
            catch (LibGit2SharpException e)
            {
                ConsoleLog(LogLevel.Error, "Couldn't push changes: " + e.Message);
            }
        }
    }
}
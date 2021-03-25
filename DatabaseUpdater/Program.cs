using DeliCounter.Backend;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

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

        private static Dictionary<string, VersionFetcher> _checkers = new()
        {
            ["bonetome.com"] = new BoneTomeVersionFetcher()
            
        };

        private static void Main(string[] args)
        {
            _checkers["github.com"] = new GitHubVersionFetcher(args[0]);
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() {Converters = new List<JsonConverter>() {new SemRangeConverter(), new SemVersionConverter()}};
            ModRepository repo = new("https://github.com/Deli-Collective/DeliCounter.Database/tree/main");

            int alreadyUpToDate = 0, updated = 0, error = 0;

            foreach (Mod mod in repo.Mods.Values)
            {
                string sourceWebsite = mod.Latest.SourceUrl.Split("/")[2];
                if (!_checkers.TryGetValue(sourceWebsite, out VersionFetcher checker))
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
            
            ConsoleLog(LogLevel.Info, $"Done! {updated} updated, {error} errors, {alreadyUpToDate} already up to date.");
        }
    }
}
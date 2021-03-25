using DeliCounter.Backend;
using DeliCounter.Backend.Models;
using LibGit2Sharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonException = System.Text.Json.JsonException;

namespace DatabaseUpdater
{
    internal class ModRepository
    {
        public enum State
        {
            Error,
            CantUpdate,
            UpToDate
        }

        private const string RepoPath = "ModRepository";

        public ModCategory[] Categories { get; private set; }
        public Dictionary<string, Mod> Mods { get; private set; } = new();
        public Repository Repo { get; private set; }
        public ApplicationData ApplicationData { get; private set; }

        public State Status { get; set; }
        public Exception Exception { get; set; }

        private string _remoteUrl;
        
        public ModRepository(string remoteUrl)
        {
            _remoteUrl = remoteUrl;
            UpdateRepo();
            ScanMods();
        }

        /// <summary>
        ///     Performs a git pull on the repo to update it
        /// </summary>
        private Exception UpdateRepo()
        {
            try
            {
                // Clone if the repo doesn't exist
                var cloneOptions = new CloneOptions {CredentialsProvider = null};
                var split = _remoteUrl.Split("/tree/");
                var repoUrl = split[0].EndsWith('/') ? split[0][..^1] : split[0];
                var branch = split.Length > 1 ? split[1] : "main";
                if (!Directory.Exists(RepoPath)) Repository.Clone(repoUrl, RepoPath, cloneOptions);
                Repo = new Repository(RepoPath);

                // Check if we're still using the same remote and if not, update it
                var remote = Repo.Network.Remotes["origin"];
                if (remote.Url != repoUrl)
                    Repo.Network.Remotes.Update(remote.Name, x =>
                    {
                        x.Url = repoUrl;
                        x.PushUrl = repoUrl;
                    });

                // Fetch to update
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                Commands.Fetch(Repo, remote.Name, refSpecs, null, null);

                // Checkout the selected branch
                Commands.Checkout(Repo, Repo.Branches[branch]);

                // No error
                return null;
            }
            catch (LibGit2SharpException e)
            {
                return e;
            }
        }

        /// <summary>
        ///     Scans the local mod repository for the categories and mods
        /// </summary>
        private Exception ScanMods()
        {
            Mods.Clear();

            // Fetch a list of all the categories in the repo
            var categoriesPath = Path.Combine(RepoPath, "categories.json");
            if (!File.Exists(categoriesPath)) return new FileNotFoundException("Categories file can not be found!");

            try
            {
                ApplicationData =
                    JsonConvert.DeserializeObject<ApplicationData>(
                        File.ReadAllText(Path.Combine(RepoPath, "application_data.json")));

                var categories = JsonConvert.DeserializeObject<ModCategory[]>(File.ReadAllText(categoriesPath));
                if (categories is null)
                {
                    Categories = Array.Empty<ModCategory>();
                    Mods = new Dictionary<string, Mod>();
                    return new JsonException("Categories file is invalid!");
                }

                Categories = categories;

                // Iterate over each category
                foreach (var category in Categories)
                {
                    // Enumerate over the directories in this category path (mods)
                    var categoryPath = Path.Combine(RepoPath, category.Path);
                    if (!Directory.Exists(categoryPath)) continue;
                    foreach (var directory in Directory.EnumerateDirectories(categoryPath))
                    {
                        // Get the GUID from the directory filename
                        var guid = Path.GetFileName(directory);
                        var mod = new Mod {Guid = guid, Category = category};

                        // Enumerate over each version of the mod
                        foreach (var versionFile in Directory.EnumerateFiles(directory))
                        {
                            // Deserialize and add the version to the mod
                            var version = JsonConvert.DeserializeObject<Mod.ModVersion>(File.ReadAllText(versionFile));
                            if (version is null) continue;
                            mod.Versions.Add(version.VersionNumber, version);
                        }

                        // If there are 0 versions for this mod pretend it doesn't exist
                        if (mod.Versions.Count == 0) continue;
                        Mods.Add(guid, mod);
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Newtonsoft.Json;
using Semver;
using Slicer.Properties;
using JsonException = System.Text.Json.JsonException;

namespace Slicer.Backend
{
    internal class ModRepository
    {
        public delegate void RepositoryUpdatedDelegate(State state, Exception e);

        public enum State
        {
            Error,
            CantUpdate,
            UpToDate
        }

        private const string RepoPath = "ModRepository";

        private static ModRepository _instance;

        public ModCategory[] Categories;
        public Dictionary<string, Mod> Mods = new();
        public List<CachedMod> InstalledMods = new();
        public Repository Repo;

        public static ModRepository Instance => _instance ??= new ModRepository();

        public event RepositoryUpdatedDelegate RepositoryUpdated;

        public State Status;

        public string ModCachePath
        {
            get
            {
                var path = Settings.Default.GameLocationOrError;
                return path is null ? null : Path.Join(path, "installed_mods.json");
            }
        }

        public ModRepository()
        {
            Settings.Default.GameLocationChanged += LoadModCache;
        }

        public void Refresh()
        {
            var updateResult = UpdateRepo();
            var scanResult = ScanMods();
            LoadModCache();

            // Check the results
            if (updateResult is null && scanResult is null)
                // Both were successful
                Status = State.UpToDate;
            else if (updateResult is not null && scanResult is null)
                // Update failed but local repo is still valid
                Status = State.CantUpdate;
            else
                // Both failed
                Status = State.Error;

            RepositoryUpdated?.Invoke(Status, updateResult ?? scanResult);
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
                if (!Directory.Exists(RepoPath))
                    Repository.Clone(Settings.Default.GitRepository, RepoPath, cloneOptions);
                Repo = new Repository(RepoPath);

                // Pull to update
                var signature = new Signature(new Identity("no_username", "unused@email.com"), DateTimeOffset.Now);
                var fetchOptions = new PullOptions {FetchOptions = new FetchOptions {CredentialsProvider = null}};
                Commands.Pull(Repo, signature, fetchOptions);

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

        private void LoadModCache()
        {
            // Clear the installed flag of any mod that has it
            foreach (var mod in Mods.Values) mod.InstalledVersion = null;

            // If the game folder is not set don't do anything
            var path = ModCachePath;
            if (path is null) return;

            // Read the mod cache (Or create it if it does not exist)
            if (!File.Exists(path))
            {
                InstalledMods = new List<CachedMod>();
                WriteCache();
            }
            else InstalledMods = JsonConvert.DeserializeObject<List<CachedMod>>(File.ReadAllText(path));

            if (InstalledMods is null) return;

            // Set the installed version on the installed mods
            foreach (var cached in InstalledMods)
                Mods.Values.First(x => x.Guid == cached.Guid).InstalledVersion = cached.Version;
        }

        public void WriteCache()
        {
            File.WriteAllText(ModCachePath, JsonConvert.SerializeObject(InstalledMods));
        }

        public bool InstallMod(Mod mod, SemVersion version = null)
        {
            // Get the version downloaded
            if (version is null) version = mod.LatestVersion;
            else if (!mod.Versions.ContainsKey(version)) return false; // TODO: Better error handling?

            // Download the dependencies
            var downloadVersion = mod.Versions[version];
            foreach (var dep in downloadVersion.Dependencies)
            {
                var result = InstallMod(Mods[dep.Key], dep.Value);
                if (!result) return false;
            }

            // Download the correct file to a temp path
            var tempPath = Path.GetTempPath();
            var downloadedFile = Path.Combine(tempPath, mod.Guid + ".tmp");

            // Keep a list of the variables used for installation
            var vars = new Dictionary<string, string>
            {
                ["DOWNLOADED_FILE"] = downloadedFile
            };

            // Perform the installations
            foreach (var step in downloadVersion.InstallationSteps)
            {
                
            }

            return true;
        }

        public void RemoveMod(Mod mod)
        {
        }
    }
}
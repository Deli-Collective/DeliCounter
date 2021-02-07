using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace Slicer.Backend
{
    class ModRepository
    {
        public enum State
        {
            Error,
            CantUpdate,
            UpToDate
        }

        private static ModRepository _instance;

        public static ModRepository Instance => _instance ??= new ModRepository();

        public delegate void RepositoryUpdatedDelegate(State state, Exception e);

        public event RepositoryUpdatedDelegate RepositoryUpdated;

        private const string RepoPath = "ModRepository";

        public ModCategory[] Categories;
        public Dictionary<string, Mod> Mods;
        public Repository Repo;

        public State Status;

        public async Task Refresh()
        {
            var updateResult = await UpdateRepo();
            var scanResult = await ScanMods();

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
        private async Task<Exception> UpdateRepo()
        {
            // TODO: This.
            return null;
        }

        /// <summary>
        ///     Scans the local mod repository for the categories and mods
        /// </summary>
        private async Task<Exception> ScanMods()
        {
            // Fetch a list of all the categories in the repo
            var categoriesPath = Path.Combine(RepoPath, "categories.json");
            if (!File.Exists(categoriesPath))
            {
                return new FileNotFoundException("Categories file can not be found!");
            }

            var categories = JsonConvert.DeserializeObject<ModCategory[]>(await File.ReadAllTextAsync(categoriesPath));
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
                // Make a new list to store the mods in this category
                var categoryMods = new List<Mod>();
                category.Mods = categoryMods;

                // Enumerate over the directories in this category path (mods)
                var categoryPath = Path.Combine(RepoPath, category.Path);
                foreach (var directory in Directory.EnumerateDirectories(categoryPath))
                {
                    // Get the GUID from the directory filename
                    var guid = Path.GetFileName(directory);
                    var mod = new Mod {Guid = guid};

                    // Enumerate over each version of the mod
                    foreach (var versionFile in Directory.EnumerateFiles(directory))
                    {
                        // Deserialize and add the version to the mod
                        var version = JsonConvert.DeserializeObject<Mod.ModVersion>(await File.ReadAllTextAsync(versionFile));
                        if (version is null) continue;
                        mod.Versions.Add(version.VersionNumber, version);
                    }

                    // If there are 0 versions for this mod pretend it doesn't exist
                    if (mod.Versions.Count == 0) continue;
                    Mods.Add(guid, mod);
                    categoryMods.Add(mod);
                }
            }

            return null;
        }
    }
}
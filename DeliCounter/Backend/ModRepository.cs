using DeliCounter.Backend.Models;
using DeliCounter.Controls;
using DeliCounter.Properties;
using LibGit2Sharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonException = System.Text.Json.JsonException;
using Range = SemVer.Range;

namespace DeliCounter.Backend
{
    internal class ModRepository
    {
        public delegate void RepositoryUpdatedDelegate();

        public enum State
        {
            Error,
            CantUpdate,
            UpToDate
        }

        private const string RepoPath = "ModRepository";

        private static ModRepository _instance;

        public ModCategory[] Categories { get; private set; }
        public Dictionary<string, Mod> Mods { get; private set; } = new();
        public Repository Repo { get; private set; }
        public ApplicationData ApplicationData { get; private set; }

        public static ModRepository Instance => _instance ??= new ModRepository();

        public event RepositoryUpdatedDelegate RepositoryUpdated;
        public event RepositoryUpdatedDelegate InstalledModsUpdated;

        public State Status { get; set; }
        public Exception Exception { get; set; }

        public static string ModCachePath
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

            LoadModCache();
            Exception = updateResult ?? scanResult;
            RepositoryUpdated?.Invoke();
            MainWindow.Instance.ModManagementDrawer.SelectedMod = null;
            App.RunInMainThread(MainWindow.Instance.ModManagementDrawer.UpdateDisplay);
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
                var split = Settings.Default.GitRepository.Split("/tree/");
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
                Commands.Checkout(Repo, Repo.Branches["refs/remotes/origin/" + branch]);

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
                            if (!Settings.Default.ShowModBetas &&
                                !string.IsNullOrEmpty(version.VersionNumber.PreRelease)) continue;
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

        /// <summary>
        ///     Reads the installed mods cache
        /// </summary>
        private void LoadModCache()
        {
            try
            {
                // Make sure we've initialized first
                if (Status == State.Error) return;

                // Clear the installed flag of any mod that has it
                foreach (var mod in Mods.Values) mod.InstalledVersion = null;

                // If the game folder is not set don't do anything
                var path = ModCachePath;
                if (path is null) return;

                // Read the mod cache (Or create it if it does not exist)
                List<CachedMod> installedMods;
                if (!File.Exists(path))
                {
                    installedMods = new List<CachedMod>();
                    WriteCache();
                }
                else
                {
                    installedMods = JsonConvert.DeserializeObject<List<CachedMod>>(File.ReadAllText(path));
                }

                if (installedMods is null) return;

                // Set the installed version on the installed mods
                foreach (var cached in installedMods)
                {
                    // If the mod isn't in the database anymore, stub it.
                    if (!Mods.ContainsKey(cached.Guid))
                        Mods[cached.Guid] = new Mod {Guid = cached.Guid};
                    var mod = Mods[cached.Guid];
                    mod.InstalledVersion = cached.Version;
                    mod.Cached = cached;

                    // The version was removed or updated
                    if (!mod.Versions.ContainsKey(mod.InstalledVersion))
                    {
                        // Just insert an empty one to allow nothing to break
                        mod.Versions.Add(mod.InstalledVersion,
                            new Mod.ModVersion
                            {
                                VersionNumber = mod.InstalledVersion,
                                Authors = Array.Empty<string>(),
                                Dependencies = new Dictionary<string, Range>(),
                                Description = "This mod has been removed from the database",
                                DownloadUrl = null,
                                IconUrl = null,
                                InstallationSteps = Array.Empty<string>(),
                                Name = cached.Guid,
                                ShortDescription = "This mod has been removed from the database",
                                SourceUrl = ""
                            });
                    }
                }
            }
            // If the mod cache is invalid let the user know. 
            catch (Exception e)
            {
                App.RunInMainThread(() =>
                {
                    new AlertDialogue("Error",
                            "Your installed mods file appears to be invalid and can not be loaded. This will probably need to be resolved manually.")
                        .ShowAsync();
                });
                DiagnosticInfoCollector.WriteExceptionToDisk(e);
            }
        }

        /// <summary>
        ///     Writes the installed mods cache to the game folder
        /// </summary>
        public void WriteCache()
        {
            if (ModCachePath is null) return;
            var installedMods = Mods.Values.Where(x => x.Cached != null).Select(x => x.Cached);
            File.WriteAllText(ModCachePath, JsonConvert.SerializeObject(installedMods.ToArray(), Formatting.Indented));
            InstalledModsUpdated?.Invoke();
        }
    }
}
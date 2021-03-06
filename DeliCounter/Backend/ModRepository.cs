using DeliCounter.Backend.Models;
using DeliCounter.Controls;
using DeliCounter.Properties;
using LibGit2Sharp;
using LibGit2Sharp.Tests.TestHelpers;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonException = System.Text.Json.JsonException;
using Range = SemVer.Range;

namespace DeliCounter.Backend
{
    public class ModRepository
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

        public ModCategory[] Categories { get; private set; } = Array.Empty<ModCategory>();
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
                var path = App.Current.Settings.GameLocationOrError;
                return path is null ? null : Path.Join(path, "installed_mods.json");
            }
        }

        public ModRepository()
        {
            App.Current.Settings.GameLocationChanged += LoadModCache;
        }

        public void Refresh()
        {
            var updateResult = UpdateRepo();

            // Check the results
            if (updateResult is null)
                // Both were successful
                Status = State.UpToDate;
            else if (updateResult is InvalidDataException)
                // Update failed but local repo is still valid
                Status = State.CantUpdate;
            else
                // Both failed
                Status = State.Error;

            LoadModCache();
            Exception = updateResult;
            RepositoryUpdated?.Invoke();
            App.RunInMainThread(MainWindow.Instance.ModManagementDrawer.ClearSelected);
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
                var split = App.Current.Settings.GitRepository.Split("/tree/");
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

                // If there's an error loading the database, go back commits until there isn't
                int commitsBack = 0;
                Exception e = ScanMods();
                if (e != null)
                {
                    App.Current.DiagnosticInfoCollector.SentryLogException(e);
                    DiagnosticInfoCollector.WriteExceptionToDisk(e);
                }

                try
                {
                    while (e != null && commitsBack < 6)
                    {
                        commitsBack++;
                        Commit currentCommit = Repo.Head.Commits.First();
                        Commands.Checkout(Repo, Repo.Head.Commits.First(x => currentCommit.Parents.Contains(x)));
                        e = ScanMods();
                    }
                } catch (InvalidOperationException ex)
                {
                    // There are no commits to go back?
                    return ex;
                }

                if (commitsBack < 6)
                    return commitsBack == 0 ? null : new InvalidDataException($"{commitsBack} commit(s) were invalid, returning to previous commit. The errors encountered can be found in the application folder.");
                else
                {
                    Categories = Array.Empty<ModCategory>();
                    Mods.Clear();
                    return new ArgumentOutOfRangeException("The current and previous 5 commits are all invalid. Stopping.");
                }
            }
            catch (LibGit2SharpException e)
            {
                // If the database failed to checkout for some reason try to scan mods anyway
                try
                {
                    ScanMods();
                } catch (Exception innerE)
                {
                    return innerE;
                }
                return new InvalidDataException(e.Message + " (Still able to read the local repo)", e);
            }
        }

        /// <summary>
        ///     Scans the local mod repository for the categories and mods
        /// </summary>
        private Exception ScanMods()
        {
            Categories ??= Array.Empty<ModCategory>();
            if (Mods is null) Mods = new Dictionary<string, Mod>();
            else Mods.Clear();
            
            // Fetch a list of all the categories in the repo
            string categoriesPath = Path.Combine(RepoPath, "categories.json");
            if (!File.Exists(categoriesPath))
                return new FileNotFoundException("Categories file can not be found!");

            try
            {
                ApplicationData =
                    JsonConvert.DeserializeObject<ApplicationData>(
                        File.ReadAllText(Path.Combine(RepoPath, "application_data.json")));

                ModCategory[] categories = JsonConvert.DeserializeObject<ModCategory[]>(File.ReadAllText(categoriesPath));
                if (categories is null)
                    return new JsonException("Categories file is invalid!");

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
                            version.Mod = mod;
                            if (version is null) continue;
                            if (version.IconUrl == "") throw new JsonException($"[{version}] Icon url cannot be empty, must be null.");
                            if ((App.Current.Settings is not null && !App.Current.Settings.ShowModBetas) &&
                                !string.IsNullOrEmpty(version.VersionNumber.PreRelease)) continue;
                            if (!App.Current.Settings.ShowModBetas && version.IsBeta) continue;
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
                Mods = new Dictionary<string, Mod>();
                Categories = Array.Empty<ModCategory>();
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

                    // The mod wasn't in the database and we need to stub it with some random info
                    if (!mod.Versions.ContainsKey(mod.InstalledVersion))
                    {
                        // Just insert an empty one to allow nothing to break
                        mod.Versions.Add(mod.InstalledVersion,
                            mod.Versions.Count == 0
                                ? new Mod.ModVersion
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
                                }
                                : new Mod.ModVersion
                                {
                                    VersionNumber = mod.InstalledVersion,
                                    Authors = mod.Latest.Authors,
                                    Dependencies = mod.Latest.Dependencies,
                                    Description = mod.Latest.Description,
                                    IconUrl = mod.Latest.IconUrl,
                                    InstallationSteps = Array.Empty<string>(),
                                    Name = mod.Latest.Name,
                                    ShortDescription = mod.Latest.ShortDescription,
                                    SourceUrl = mod.Latest.SourceUrl
                                });
                    }
                }
            }
            // If the mod cache is invalid let the user know. 
            catch (Exception e)
            {
                if (!App.Current.AreDialogsQueued) App.RunInMainThread(() =>
                {
                    App.Current.QueueDialog(
                        new AlertDialogue("Error",
                            "Your installed mods file appears to be invalid and can not be loaded. This will probably need to be resolved manually.")
                    );
                });
                DiagnosticInfoCollector.WriteExceptionToDisk(e);
                App.Current.DiagnosticInfoCollector.SentryLogException(e);
            }
        }

        public void Reset()
        {
            if (Repo is not null) Repo.Dispose();
            if (Directory.Exists(RepoPath)) DirectoryHelper.DeleteDirectory(RepoPath);
            Refresh();
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
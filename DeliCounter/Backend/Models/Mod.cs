using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Range = SemVer.Range;
using Version = SemVer.Version;

namespace DeliCounter.Backend
{
    /// <summary>
    ///     Represents a mod
    /// </summary>
    public class Mod
    {
        /// <summary>
        ///     All versions of the mod found in the database
        /// </summary>
        public Dictionary<Version, ModVersion> Versions = new();

        /// <summary>
        ///     Mod GUID.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        ///     Category this mod is in
        /// </summary>
        public ModCategory Category { get; set; }

        /// <summary>
        ///     Retrieves the latest version number from the database
        /// </summary>
        public Version LatestVersion =>
            Versions.Keys.Where(x => Versions[x].DownloadUrl is not null || Versions.Count == 1).Max();

        /// <summary>
        ///     Retrieves the latest version from the database
        /// </summary>
        public ModVersion Latest => Versions[LatestVersion];

        /// <summary>
        ///     True if the mod is installed locally
        /// </summary>
        public bool IsInstalled => InstalledVersion is not null;

        public bool IsInstalledVersionInDatabase =>
            Versions.ContainsKey(InstalledVersion) && !string.IsNullOrEmpty(Installed.DownloadUrl);

        /// <summary>
        ///     True if the local version matches the latest version
        /// </summary>
        public bool UpToDate => IsInstalled && Equals(InstalledVersion, LatestVersion);

        /// <summary>
        ///     Retrieves the installed mod version number
        /// </summary>
        public Version InstalledVersion { get; set; }

        /// <summary>
        ///  Retrieves the mod cache entry for this mod
        /// </summary>
        public CachedMod Cached { get; set; }

        /// <summary>
        ///     Retrieves the installed version
        /// </summary>
        public ModVersion Installed => Versions.TryGetValue(InstalledVersion, out ModVersion version) ? version : null;

        public IEnumerable<Mod> InstalledDirectDependents =>
            ModRepository.Instance.Mods.Values.Where(x =>
                x.IsInstalled && x.Installed.Dependencies.ContainsKey(Guid));

        public IEnumerable<Mod> InstalledDependents
        {
            get
            {
                var list = new List<Mod>();
                foreach (var dep in InstalledDirectDependents)
                {
                    list.Add(dep);
                    list.AddRange(dep.InstalledDependents);
                }

                return list.Distinct();
            }
        }

        /// <summary>
        ///     Represents a specific version of a mod
        /// </summary>
        public class ModVersion
        {
            [JsonIgnore]
            public Mod Mod { get; set; }

            /// <summary>
            ///     Semantic Version number of this version of the mod
            /// </summary>
            public Version VersionNumber { get; set; }

            /// <summary>
            ///     Name of the mod
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            ///     Long description for the mod page which supports formatting
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            ///     Short description to be used in lists
            /// </summary>
            public string ShortDescription { get; set; }

            /// <summary>
            ///     URL for the mod's icon
            /// </summary>
            public string IconUrl { get; set; }

            /// <summary>
            ///     URL for the mod's preview image
            /// </summary>
            public string PreviewImageUrl { get; set; }

            /// <summary>
            ///     Source URL for the mod, e.g. a homepage, Git repo or other website link
            /// </summary>
            public string SourceUrl { get; set; }

            /// <summary>
            ///     Authors
            /// </summary>
            public string[] Authors { get; set; }

            /// <summary>
            ///     URL to the file that will be downloaded
            /// </summary>
            public string DownloadUrl { get; set; }

            /// <summary>
            ///     List of GUID dependencies for this mod.
            /// </summary>
            public Dictionary<string, Range> Dependencies { get; set; }

            /// <summary>
            ///     List of steps required for installing this mod
            /// </summary>
            public string[] InstallationSteps { get; set; }

            /// <summary>
            ///     List of tags for this mod. Used to identify incompatibilities.
            /// </summary>
            public string[] Tags { get; set; }

            /// <summary>
            ///     A list of tags which this mod is not compatible with. Used for mods which you can only have one of installed.
            /// </summary>
            public string[] IncompatibleTags { get; set; }

            /// <summary>
            ///     True if the mod should only be shown to people who have the beta option enbled
            /// </summary>
            public bool IsBeta { get; set; }

            [JsonIgnore]
            public IEnumerable<Mod> IncompatibleInstalledMods => IncompatibleTags == null
                ? Array.Empty<Mod>()
                : ModRepository.Instance.Mods.Values
                    .Where(x => x.IsInstalled && x.Installed != this)
                    .Where(x =>
                        IncompatibleTags.Contains(x.Guid) || (
                        x.Installed.Tags != null &&
                        IncompatibleTags.Any(tag => x.Installed.Tags.Contains(tag))));


            public bool MatchesQuery(string query)
            {
                return Name.ToLower().Contains(query) ||
                       Description.ToLower().Contains(query) ||
                       ShortDescription.ToLower().Contains(query) ||
                       Authors.Any(x => x.ToLower().Contains(query));
            }

            public override string ToString()
            {
                return $"{Mod.Guid} {VersionNumber}";
            }
        }

        public bool MatchesQuery(string query)
        {
            return Guid.ToLower().Contains(query) ||
                   Latest.MatchesQuery(query);
        }

        public override string ToString()
        {
            return IsInstalled ? $"{Guid} {InstalledVersion}" : Guid;
        }
    }

    /// <summary>
    ///     Represents a mod in the cache. It's just a couple fields since we can then pull the rest of the data from the actual database
    /// </summary>
    public class CachedMod
    {
        public string Guid { get; set; }
        public string VersionString { get; set; }
        public string[] Files { get; set; }

        [JsonIgnore] public Version Version => Version.Parse(VersionString);
    }
}
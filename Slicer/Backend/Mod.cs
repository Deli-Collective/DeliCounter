using System.Collections.Generic;
using System.Linq;
using Semver;

namespace Slicer.Backend
{
    /// <summary>
    ///     Represents a mod
    /// </summary>
    public class Mod
    {
        /// <summary>
        ///     Represents a specific version of a mod
        /// </summary>
        public class ModVersion
        {
            /// <summary>
            ///     Semantic Version number of this version of the mod
            /// </summary>
            public SemVersion VersionNumber { get; set; }
            
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
            ///     Source URL for the mod, e.g. a homepage, Git repo or other website link
            /// </summary>
            public string SourceUrl { get; }
            
            // TODO: Installation and removal data for each version
        }

        /// <summary>
        ///     Mod GUID.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        ///     All versions of the mod found in the database
        /// </summary>
        public Dictionary<SemVersion, ModVersion> Versions;

        /// <summary>
        ///     Retrieves the latest version number from the database
        /// </summary>
        public SemVersion LatestVersion => Versions.Keys.Max();

        /// <summary>
        ///     Retrieves the latest version from the database
        /// </summary>
        public ModVersion Latest => Versions[LatestVersion];

        /// <summary>
        ///     True if the mod is installed locally
        /// </summary>
        public bool IsInstalled => InstalledVersion is not null;

        /// <summary>
        ///     True if the local version matches the latest version
        /// </summary>
        public bool UpToDate => IsInstalled && SemVersion.Equals(InstalledVersion, LatestVersion);

        /// <summary>
        ///     Retrieves the installed mod version number
        /// </summary>
        public SemVersion InstalledVersion { get; set; }

        /// <summary>
        ///     Retrieves the installed version
        /// </summary>
        public ModVersion Installed => Versions[InstalledVersion];
    }
}
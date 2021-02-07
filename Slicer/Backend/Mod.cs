using System.Collections.Generic;
using System.Linq;
using Semver;

namespace Slicer.Backend
{
    public class Mod
    {
        public class ModVersion
        {
            public string Name { get; }
            public string Description { get; }
            public string ShortDescription { get; }
            public string IconUrl { get; }
            public string SourceUrl { get; }

            // TODO: Installation and removal data for each version
        }

        public string Guid { get; set; }
        public Dictionary<SemVersion, ModVersion> Versions;

        public SemVersion LatestVersion => Versions.Keys.Max();
        public ModVersion Latest => Versions[LatestVersion];

        public bool IsInstalled { get; set; }
        public SemVersion InstalledVersion { get; set; }
        public ModVersion Installed => Versions[InstalledVersion];
    }
}
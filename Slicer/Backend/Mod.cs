using Semver;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Deli_Counter.Backend
{
    public class Mod
    {
        public class ModVersion
        {
            public string IconUrl { get; set; }
            public string Name { get; set; }
            public string ShortDescription { get; set; }
            public string Description { get; set; }
            public string[] Authors { get; set; }
            public SemVersion Version { get; set; }
            public string Source { get; set; }
            public Mod Mod { get; set; }
        }

        public string Guid { get; set; }
        public ModVersion[] Versions { get; set; }
        public ModCategory Category { get; set; }

        public SemVersion LatestVersionNumber => Versions.Max(x => x.Version);
        public ModVersion LatestVersion => Versions.FirstOrDefault(x => x.Version == LatestVersionNumber);

        public bool Installed { get; set; }
        public SemVersion InstalledVersionNumber { get; set; }
        public ModVersion InstalledVersion => Versions.FirstOrDefault(x => x.Version == InstalledVersionNumber);
    }
}

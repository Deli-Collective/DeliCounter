using Semver;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Deli_Counter.Backend
{
    public class Mod
    {
        public class ModVersion
        {
            public string Guid { get; set; }
            public string IconUrl { get; set; }
            public string Name { get; set; }
            public string ShortDescription { get; set; }
            public string Description { get; set; }
            public string[] Authors { get; set; }
            public SemVersion Version { get; set; }
            public string Source { get; set; }
        }

        public ModVersion[] Versions { get; set; }
        public SemVersion LatestVersion { get; set; }
        public SemVersion InstalledVersion { get; set; }
        public ModCategory Category { get; set; }

        public ModVersion Latest => Versions.FirstOrDefault(x => x.Version == LatestVersion);
        public ModVersion Installed => Versions.FirstOrDefault(x => x.Version == InstalledVersion);
    }
}

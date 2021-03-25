using DeliCounter.Backend;
using SemVer;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseUpdater
{
    public abstract class VersionFetcher
    {
        public abstract bool Versioned { get; }

        public abstract Task<FetchedVersion> GetLatestVersion(Mod.ModVersion mod);

        protected Version SanitizeVersionString(string version)
        {
            string sanitized = Regex.Replace(version, "[^0-9.]", "");
            switch (sanitized.Count(x => x == '.'))
            {
                case 1:
                    sanitized += ".0";
                    break;
                case 3:
                    sanitized = sanitized[..sanitized.LastIndexOf('.')];
                    break;
            }

            return Version.Parse(sanitized);
        }
    }

    public class FetchedVersion
    {
        public Version Version { get; set; }
        public string DownloadUrl { get; set; }
        
    }
}

using SemVer;

namespace DeliCounter.Backend.Models
{
    public class ApplicationData
    {
        /// <summary>
        ///     Latest version of this application
        /// </summary>
        public Version LatestApplicationVersion { get; set; }

        /// <summary>
        ///     Short update notes for this version
        /// </summary>
        public string UpdateText { get; set; }

        /// <summary>
        ///     Link to the latest release
        /// </summary>
        public string ReleaseLink { get; set; }
    }
}
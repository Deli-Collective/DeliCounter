using DeliCounter.Backend;
using HtmlAgilityPack;
using System.Linq;
using System.Threading.Tasks;
using Version = SemVer.Version;

namespace DatabaseUpdater
{
    public class BoneTomeVersionFetcher : VersionFetcher
    {
        public override bool Versioned => false;

        public override async Task<FetchedVersion> GetLatestVersion(Mod.ModVersion mod)
        {
            HtmlDocument page = await new HtmlWeb().LoadFromWebAsync(mod.SourceUrl);

            HtmlNode modData = page.GetElementbyId("mod-data");
            string version = modData.SelectSingleNode("div[8]/div[2]").InnerHtml;

            string downloadUrl = "https://bonetome.com" + page.GetElementbyId("download-button").ParentNode.Attributes["href"].Value;

            return new FetchedVersion
            {
                Version = SanitizeVersionString(version),
                DownloadUrl = downloadUrl
            };
        }
    }
}
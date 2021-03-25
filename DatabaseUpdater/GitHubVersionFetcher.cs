using DeliCounter.Backend;
using Octokit;
using SemVer;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Version = SemVer.Version;

namespace DatabaseUpdater
{
    public class GitHubVersionFetcher : VersionFetcher
    {
        private readonly GitHubClient _client;

        public override bool Versioned => true;

        public GitHubVersionFetcher(GitHubClient client)
        {
            _client = client;
        }

        public override async Task<FetchedVersion> GetLatestVersion(Mod.ModVersion mod)
        {
            string[] split = mod.SourceUrl.Split("/");
            Release latest = await _client.Repository.Release.GetLatest(split[3], split[4]);
            Version version = SanitizeVersionString(latest.TagName);

            // Do some filtering to determine the asset to use if there is more than one
            ReleaseAsset asset;
            if (latest.Assets.Count > 1)
            {
                ReleaseAsset[] assets = latest.Assets
                    .Where(x =>
                        Regex.IsMatch(
                            mod.DownloadUrl,
                            Regex.Replace(Regex.Escape(x.Name), @"\d(?:\\\.\d)+", ".*")
                        )
                    ).ToArray();

                if (assets.Length > 1)
                {
                    throw new Exception("There was more than one asset and the correct one could not be determined.");
                }

                asset = assets[0];
            }
            else asset = latest.Assets[0];

            return new FetchedVersion {Version = version, DownloadUrl = asset.BrowserDownloadUrl};
        }
    }
}
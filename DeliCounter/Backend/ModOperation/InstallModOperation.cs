using DeliCounter.Properties;
using Sentry;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Version = SemVer.Version;

namespace DeliCounter.Backend.ModOperation
{
    internal class InstallModOperation : ModOperation
    {
        private readonly WebClient _webClient = new();

        private readonly Dictionary<string, string> _vars = new();

        private readonly Mod.ModVersion _version;

        private readonly List<string> _installedFiles = new();

        public InstallModOperation(Mod mod, Version versionNumber) : base(mod, versionNumber)
        {
            _version = mod.Versions[versionNumber];
        }

        internal override async Task Run()
        {
            await base.Run();

            // Make sure we have the game directory
            var gameDir = Settings.Default.GameLocationOrError;
            if (gameDir is null) return;

            // Set some things up
            var t = new System.Timers.Timer { AutoReset = false, Interval = 15000 };
            t.Elapsed += (sender, args) =>
            {
                _webClient.CancelAsync();
            };
            ProgressDialogueCallback(0, $"Downloading {_version.Name}... (0.00 MB)");
            _webClient.DownloadProgressChanged += (sender, args) =>
            {
                t.Interval = 15000;
                var totalMegabytes = args.BytesReceived / 1000000d;
                ProgressDialogueCallback(args.ProgressPercentage / 200d,
                    $"Downloading {_version.Name}... ({totalMegabytes:#.##} MB)");
            };


            // Download the file
            var downloadedPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            t.Start();
            try
            {
                await _webClient.DownloadFileTaskAsync(_version.DownloadUrl, downloadedPath);
            }
            catch (TaskCanceledException)
            {
                // Our timeout timer was invoked and we stopped the download.
                Message = "Download went more than 15 seconds without receiving any new data.";
                Completed = false;
                return;
            }
            catch (WebException e)
            {
                // The download couldn't be completed for some reason.
                Message = $"Could not download the file: {e.Message}";
                Completed = false;
                return;
            }
            finally
            {
                t.Dispose();
            }


            // Execute the install steps
            ProgressDialogueCallback(0.5, $"Installing {_version.Name}");

            // Set the starting arguments
            _vars["IMPLICIT"] = downloadedPath;
            _vars["DOWNLOADED_FILE"] = downloadedPath;
            _vars["GAME_DIR"] = gameDir;

            // Execute the installation steps
            foreach (var step in _version.InstallationSteps)
            {
                var args = step.Split(" ");
                for (var i = 1; i < args.Length; i++) args[i] = ExpandArgs(args[i]);

                switch (args[0])
                {
                    case "extract":
                        await Task.Run(() => Extract(args));
                        break;
                    case "move":
                        await Task.Run(() => Move(args));
                        break;
                    case "mkdir":
                        await Task.Run(() => Mkdir(args));
                        break;
                }

                // If a step failed then exit early
                if (!Completed) break;
            }

            // Remove the temp file to not take up storage :)
            if (File.Exists(downloadedPath)) File.Delete(downloadedPath);

            // Update the repo
            Mod.InstalledVersion = _version.VersionNumber;
            Mod.Cached = new CachedMod
            {
                Guid = Mod.Guid,
                VersionString = _version.VersionNumber.ToString(),
                Files = _installedFiles.Select(x =>
                    x.Replace('\\', '/')
                        .Replace(_vars["GAME_DIR"].Replace('\\', '/'), ""))
                    .Select(x => x[0] == '/' ? x[1..] : x)
                    .ToArray()
            };
        }

        private string ExpandArgs(string arg)
        {
            return _vars.Aggregate(arg, (current, var) => current.Replace("$" + var.Key, var.Value));
        }

        private void Extract(string[] args)
        {
            try
            {
                var archive = ArchiveFactory.Open(_vars["IMPLICIT"]);
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(args[1], new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                    _installedFiles.Add(Path.Combine(args[1], entry.Key));
                }

                archive.Dispose();
            } catch (InvalidOperationException)
            {
                // Zip file was not valid?
                Completed = false;
                Message = "Downloaded file was not a valid zip. Incomplete download? Try again";
            }

        }

        private void Move(string[] args)
        {
            string source, destination;
            switch (args.Length)
            {
                case 2:
                    source = _vars["IMPLICIT"];
                    destination = Path.Combine(_vars["GAME_DIR"], args[1]);
                    break;
                case 3:
                    source = args[1];
                    destination = Path.Combine(_vars["GAME_DIR"], args[2]);
                    break;
                default:
                    // Something's borked
                    return;
            }


            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            if (Directory.Exists(source))
                Directory.Move(source, destination);
            else if (File.Exists(source))
                File.Move(source, destination, true);

            _installedFiles.Add(destination);
        }

        private static void Mkdir(string[] args)
        {
            Directory.CreateDirectory(args[1]);
        }
    }
}
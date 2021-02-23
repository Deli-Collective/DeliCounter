using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Printing;
using System.Threading;
using DeliCounter.Properties;
using Newtonsoft.Json.Bson;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace DeliCounter.Backend.ModOperation
{
    internal class InstallModOperation : ModOperation
    {
        private readonly WebClient _webClient = new();

        private readonly Dictionary<string, string> _vars = new();

        private readonly Mod.ModVersion _version;

        public InstallModOperation(Mod mod) : base(mod)
        {
            _version = mod.Latest;
        }

        internal override void Run()
        {
            // Make sure we have the game directory
            var gameDir = Settings.Default.GameLocationOrError;
            if (gameDir is null) return;

            // Set some things up
            ProgressDialogueCallback(0, $"Downloading {_version.Name}...");
            _webClient.DownloadProgressChanged += (sender, args) => ProgressDialogueCallback(args.ProgressPercentage / 200d, $"Downloading {_version.Name}...");

            // Download the file
            var downloadedPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            _webClient.DownloadFile(_version.DownloadUrl, downloadedPath);

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
                        Extract(args);
                        break;
                    case "move":
                        Move(args);
                        break;
                    case "mkdir":
                        Mkdir(args);
                        break;
                }
            }

            // Update the repo
            Mod.InstalledVersion = _version.VersionNumber;
        }

        private string ExpandArgs(string arg)
        {
            return _vars.Aggregate(arg, (current, var) => current.Replace("$" + var.Key, var.Value));
        }

        private void Extract(string[] args)
        {
            var archive = ArchiveFactory.Open(_vars["IMPLICIT"]);
            var entries = archive.Entries.Where(entry => !entry.IsDirectory).ToArray();
            foreach (var entry in entries) entry.WriteToDirectory(args[1], new ExtractionOptions {ExtractFullPath = true, Overwrite = true});
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
                File.Move(source, destination);
        }

        private void Mkdir(string[] args)
        {
            Directory.CreateDirectory(args[1]);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Printing;
using System.Threading;
using Newtonsoft.Json.Bson;
using SharpCompress.Archives;
using SharpCompress.Common;
using Slicer.Properties;

namespace Slicer.Backend.ModOperation
{
    internal class InstallModOperation : ModOperation
    {
        private static readonly WebClient WebClient = new();

        private Dictionary<string, string> _vars = new();

        public InstallModOperation(Mod mod) : base(mod)
        {
        }

        internal override void Run()
        {
            // Make sure we have the game directory
            var gameDir = Settings.Default.GameLocationOrError;
            if (gameDir is null) return;

            // Download the file
            var version = Mod.Latest;
            ProgressDialogueCallback(0, $"Downloading {version.Name}...");
            var downloadedPath = Path.Combine(Path.GetTempPath(), version.DownloadFilename);
            WebClient.DownloadFile(version.DownloadUrl, downloadedPath);

            // Execute the install steps
            ProgressDialogueCallback(0.5, $"Installing {version.Name}");

            // Set the starting arguments
            _vars["IMPLICIT"] = downloadedPath;
            _vars["DOWNLOADED_FILE"] = downloadedPath;
            _vars["GAME_DIR"] = gameDir;

            // Execute the installation steps
            foreach (var step in version.InstallationSteps)
            {
                var args = step.Split(" ");
                for (var i = 1; i < args.Length; i++) args[i] = ExpandArgs(args[i]);

                switch (args[0])
                {
                    case "extract":
                        Extract(args);
                        break;
                    case "copy":
                        Move(args);
                        break;
                    case "mkdir":
                        Mkdir(args);
                        break;
                }
            }

            // Update the repo
            Mod.InstalledVersion = version.VersionNumber;
        }

        string ExpandArgs(string arg) =>
            _vars.Aggregate(arg, (current, var) => current.Replace("$" + var.Key, var.Value));

        private void Extract(string[] args)
        {
            var archive = ArchiveFactory.Open(_vars["IMPLICIT"]);
            foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                entry.WriteToDirectory(args[1], new ExtractionOptions {ExtractFullPath = true, Overwrite = true});
        }

        private void Move(string[] args)
        {
            string source, destination;
            switch (args.Length)
            {
                case 2:
                    source = _vars["IMPLICIT"];
                    destination = args[1];
                    break;
                case 3:
                    source = args[1];
                    destination = args[2];
                    break;
                default:
                    // Something's borked
                    return;
            }

            if (Directory.Exists(source))
                Directory.Move(source, destination);
            else if (File.Exists(source))
                File.Move(source, destination);
        }

        private void Mkdir(string[] args) => Directory.CreateDirectory(args[1]);
    }
}
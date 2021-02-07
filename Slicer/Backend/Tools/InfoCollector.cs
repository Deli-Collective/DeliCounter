using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using Slicer.Controls;

namespace Slicer.Backend
{
    public static class InfoCollector
    {
        public static void CollectAll()
        {
            // Create the new zip archive
            var archiveFileName = $"SlicerDiagnostics_{DateTime.Now:yy-MM-dd_hh-mm-ss}.zip";
            using var fileStream =
                new FileStream(Path.Combine(SpecialDirectories.Desktop, archiveFileName), FileMode.Create);
            using var zip = new ZipArchive(fileStream, ZipArchiveMode.Create);

            void WriteToArchiveFile(string fileName, string text)
            {
                var entry = zip.CreateEntry(fileName);
                using var writer = new StreamWriter(entry.Open());
                writer.Write(text);
            }

            var diagnosticText = "== Diagnostic Info ==\n" +
                                 $"Generated at: {DateTime.Now}\n" +
                                 $"Game Directory: {GameLocator.GameDirectory}\n" +
                                 "\n== Slicer Git Info ==\n" + ApplicationGitInfo.Text;
            WriteToArchiveFile("SlicerDiagnostics.txt", diagnosticText);

            // If we don't know where the game is that's fine just skip the rest
            if (string.IsNullOrEmpty(GameLocator.GameDirectory)) return;
            WriteToArchiveFile("tree.txt", GenerateTree());
            WriteToArchiveFile("installed_mods.json", File.ReadAllText(GameLocator.ModCache));

            // If the BepInEx log file exists, include that too
            var logPath = Path.Combine(GameLocator.GameDirectory, "BepInEx", "LogOutput.log");
            if (File.Exists(logPath)) WriteToArchiveFile("LogOutput.log", File.ReadAllText(logPath));
        }

        /// <summary>
        /// Runs the tree command on the H3 directory for additional debugging
        /// </summary>
        public static string GenerateTree()
        {
            // If the H3 folder isn't found, just return
            if (string.IsNullOrEmpty(GameLocator.GameDirectory))
            {
                return "";
            }

            // Create a string builder and output the current time
            var sb = new StringBuilder();
            sb.AppendLine($"Generated at {DateTime.Now}");

            // Start the tree command and wait for it to exit
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe", $"/C tree /F /A {GameLocator.GameDirectory}")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            proc.Start();
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();


            // Iterate over the output lines and remove the h3vr_Data folder from it
            var skip = false;
            foreach (var line in output.Split("\r\n"))
            {
                if (line == "\\---h3vr_Data")
                {
                    skip = true;
                    sb.AppendLine(line + " (TRUNCATED)");
                }
                else if (skip && line.StartsWith("\\---")) skip = false;

                if (!skip) sb.AppendLine(line);
            }

            return sb.ToString();
        }
    }
}
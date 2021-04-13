using DeliCounter.Controls;
using DeliCounter.Properties;
using Microsoft.VisualBasic.FileIO;
using Sentry;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Windows;

namespace DeliCounter.Backend
{
    public class DiagnosticInfoCollector
    {
        private SteamAppLocator SteamAppLocator { get; }

        public DiagnosticInfoCollector(SteamAppLocator appLocator)
        {
            SteamAppLocator = appLocator;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        private IDisposable _sentry;

        public IDisposable InitSentry()
        {
            _sentry = SentrySdk.Init(o =>
            {
                o.Dsn = "https://bcfd3132000f420986e6c816e9d8a621@o567748.ingest.sentry.io/5712026";
                o.Release = $"deli-counter@{Version.Parse($"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}")}";
                o.ShutdownTimeout = TimeSpan.FromSeconds(5);
            });
            return _sentry;
        }

        public string CollectAll()
        {
            // Create the new zip archive
            var archiveFileName = Path.Combine(SpecialDirectories.Desktop, $"DeliCounterDiagnostics_{DateTime.Now:yy-MM-dd_hh-mm-ss}.zip");
            using var fileStream = new FileStream(archiveFileName, FileMode.Create);
            using var zip = new ZipArchive(fileStream, ZipArchiveMode.Create);

            // Local method to reduce code repetition
            void WriteToArchiveFile(string fileName, string text)
            {
                var entry = zip.CreateEntry(fileName);
                using var writer = new StreamWriter(entry.Open());
                writer.Write(text);
            }

            var diagnosticText = "== Diagnostic Info ==\n" +
                                 $"Generated at: {DateTime.Now}\n" +
                                 $"Game Directory: {SteamAppLocator.AppLocation}\n" +
                                 $"Mod Repository: {Settings.Default.GitRepository}\n" +
                                 "\n== DeliCounter Git Info ==\n" + ApplicationGitInfo.Text;
            WriteToArchiveFile("DeliCounterVersion.txt", diagnosticText);

            // Any exception files in the application folder are also fair game
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var file in Directory.EnumerateFiles(path, "Exception_*.txt"))
                WriteToArchiveFile(Path.GetFileName(file), File.ReadAllText(file));

            // If we don't know where the game is that's fine just skip the rest
            if (!string.IsNullOrEmpty(SteamAppLocator.AppLocation))
            {

                // Do a tree and write the installed mods file into the archive
                WriteToArchiveFile("tree.txt", GenerateTree());
                WriteToArchiveFile("installed_mods.json", File.ReadAllText(Path.Combine(SteamAppLocator.AppLocation, Constants.InstalledModsCache)));

                // If the BepInEx log file exists, include that too
                var logPath = Path.Combine(SteamAppLocator.AppLocation, "BepInEx", "LogOutput.log");
                if (File.Exists(logPath)) WriteToArchiveFile("LogOutput.log", File.ReadAllText(logPath));

                // Collect any BepInEx preloader errors
                foreach (var file in Directory.EnumerateFiles(SteamAppLocator.AppLocation, "preloader_*.log"))
                    WriteToArchiveFile(Path.GetFileName(file), File.ReadAllText(file));
            }

            return archiveFileName;
        }

        /// <summary>
        ///     Runs the tree command on the H3 directory for additional debugging
        /// </summary>
        public string GenerateTree()
        {
            // If the H3 folder isn't found, just return
            if (string.IsNullOrEmpty(SteamAppLocator.AppLocation)) return "";

            // Create a string builder and output the current time
            var sb = new StringBuilder();
            sb.AppendLine($"Generated at {DateTime.Now}");

            // Start the tree command and wait for it to exit
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe", $"/C tree /F /A \"{SteamAppLocator.AppLocation}\"")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();


            // Iterate over the output lines and remove the h3vr_Data folder from it
            var skip = false;
            foreach (var line in output.Split("\r\n"))
            {
                if (line.Contains("h3vr_Data") || line.Contains("ModRepository"))
                {
                    skip = true;
                    sb.AppendLine(line + " (TRUNCATED)");
                }
                else if (skip && line.TrimEnd() == "|")
                {
                    skip = false;
                }

                if (!skip) sb.AppendLine(line);
            }

            return sb.ToString();
        }

        public static void WriteExceptionToDisk(Exception e)
        {
            try
            {
                var filename = $"Exception_{DateTime.Now:yy-MM-dd_hh-mm-ss}.txt";
                File.WriteAllText(filename, e.ToString());
            } catch (UnauthorizedAccessException)
            {
                // Ignored.
            }
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception) e.ExceptionObject;
            WriteExceptionToDisk(ex);

            if (e.IsTerminating)
            {
                string filename = CollectAll();

                _sentry.Dispose();
                using (InitSentry())
                {
                    SentrySdk.WithScope(scope => {
                        scope.AddAttachment(filename);
                        SentrySdk.CaptureException(ex);
                    });
                }

                MessageBox.Show("Something went wrong and the application needs to exit. Information about this error (installed mods, error details) were sent to the developer", "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
                SentrySdk.FlushAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            }
        }
    }
}
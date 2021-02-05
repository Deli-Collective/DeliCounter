using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Deli_Counter.Backend
{
    class Utilities
    {
        private static bool scanned = false;
        private static string _gameLocation = "";

        internal static string GameDirectory
        {
            get
            {
                // If the game isn't found, return null
                if (scanned) return string.IsNullOrEmpty(_gameLocation) ? null : _gameLocation;
                scanned = true;

                // Get the main steam installation location via registry.
                var steamDir = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", "") as string;
                if (string.IsNullOrEmpty(steamDir)) steamDir = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", "") as string;
                if (!string.IsNullOrEmpty(steamDir))
                {

                    // Check main steamapps library folder for h3 manifest.
                    var result = "";
                    if (File.Exists(Path.Combine(steamDir, @"steamapps\appmanifest_450540.acf"))) result = Path.Combine(steamDir, @"steamapps\common\H3VR\");
                    else
                    {
                        // We didn't find it, look at other library folders by lazily parsing libraryfolders.
                        var libraryFolders = Path.Combine(steamDir, @"steamapps\libraryfolders.vdf");
                        foreach (var ii in File.ReadAllLines(libraryFolders).Skip(4).Where(x => x.Length != 0 && x[0] != '}').Select(x => x.Split('\t')[3].Trim('"').Replace(@"\\", @"\")).Where(ii => File.Exists(ii + @"\steamapps\appmanifest_450540.acf")))
                        {
                            result = Path.Combine(ii, @"steamapps\common\H3VR\");
                            break;
                        }
                    }

                    _gameLocation = result;
                    if (!string.IsNullOrEmpty(_gameLocation)) return _gameLocation;
                }
                return null;
            }
        }

        public static string ExecutablePath => string.IsNullOrEmpty(GameDirectory) ? null : Path.Combine(GameDirectory, "h3vr.exe");
        public static string ModCache => string.IsNullOrEmpty(GameDirectory) ? null : Path.Combine(GameDirectory, "installed_mods.json");
    }
}

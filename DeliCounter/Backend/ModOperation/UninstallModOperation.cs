using System.IO;
using System.Threading.Tasks;
using DeliCounter.Properties;

namespace DeliCounter.Backend.ModOperation
{
    internal class UninstallModOperation : ModOperation
    {
        public UninstallModOperation(Mod mod) : base(mod)
        {
        }

        internal override async Task Run()
        {
            var cached = Mod.Cached;

            await Task.Run(() =>
            {
                var gameLocation = Settings.Default.GameLocationOrError;
                foreach (var item in cached.Files)
                {
                    var path = Path.Combine(gameLocation, item);
                    if (string.IsNullOrWhiteSpace(gameLocation) || string.IsNullOrWhiteSpace(item) ||
                        !path.Contains(gameLocation)) return;

                    if (File.Exists(path)) File.Delete(path);
                    else if (Directory.Exists(path)) Directory.Delete(path, true);
                }

                DeleteEmptyDirectories(gameLocation);
            });
            Mod.InstalledVersion = null;
            Mod.Cached = null;
            Completed = true;
        }

        private static void DeleteEmptyDirectories(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                DeleteEmptyDirectories(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }
    }
}
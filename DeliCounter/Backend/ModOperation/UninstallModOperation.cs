using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeliCounter.Properties;

namespace DeliCounter.Backend.ModOperation
{
    class UninstallModOperation : ModOperation
    {
        public UninstallModOperation(Mod mod) : base(mod)
        {
        }

        internal override void Run()
        {
            var version = Mod.Latest;

            var gameLocation = Settings.Default.GameLocationOrError;
            for (var i = 0; i < version.RemovalPaths.Length; i++)
            {
                var item = version.RemovalPaths[i];
                var path = Path.Combine(gameLocation, item);
                if (string.IsNullOrWhiteSpace(gameLocation) || string.IsNullOrWhiteSpace(item) ||
                    !path.Contains(gameLocation)) return;

                ProgressDialogueCallback((1d / version.RemovalPaths.Length) * i, $"Uninstalling {version.Name}: {item}");

                if (File.Exists(path)) File.Delete(path);
                else if (Directory.Exists(path)) Directory.Delete(path, true);
            }

            Mod.InstalledVersion = null;
        }
    }
}
using System;
using System.Threading;

namespace Slicer.Backend.ModOperation
{
    internal class InstallModOperation : ModOperation
    {
        public InstallModOperation(Mod mod) : base(mod)
        {
        }

        internal override void Run()
        {
            var version = Mod.Latest;
            ProgressDialogueCallback(0, $"Downloading {version.Name}...");

            Thread.Sleep(500);

            ProgressDialogueCallback(0.5, $"Installing {version.Name}");

            Thread.Sleep(500);
        }
    }
}

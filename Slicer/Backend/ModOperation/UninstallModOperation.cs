using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Slicer.Backend.ModOperation
{
    class UninstallModOperation : ModOperation
    {
        public UninstallModOperation(Mod mod) : base(mod)
        {
        }

        internal override void Run()
        {
            var version = Mod.Latest;
            ProgressDialogueCallback(0, $"Uninstalling {version.Name}...");

            Thread.Sleep(1000);
        }
    }
}

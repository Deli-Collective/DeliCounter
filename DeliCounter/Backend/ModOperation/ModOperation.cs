using System;
using System.Threading.Tasks;

namespace DeliCounter.Backend.ModOperation
{
    internal abstract class ModOperation
    {
        internal Mod Mod { get; }

        internal Action<double, string> ProgressDialogueCallback { get; set; }

        internal ModOperation(Mod mod)
        {
            Mod = mod;
        }

        internal abstract Task Run();
    }
}
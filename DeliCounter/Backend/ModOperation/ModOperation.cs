using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DeliCounter.Backend.ModOperation
{
    internal abstract class ModOperation
    {
        internal Mod Mod { get; }

        internal List<string> InstalledFiles { get; } = new();

        internal Action<double, string> ProgressDialogueCallback { get; set; }

        protected internal bool Completed { get; protected set; }

        protected internal string Message { get; protected set; }

        internal ModOperation(Mod mod)
        {
            Mod = mod;
        }

        internal abstract Task Run();
    }
}
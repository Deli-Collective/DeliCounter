using Sentry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Version = SemVer.Version;

namespace DeliCounter.Backend.ModOperation
{
    public abstract class ModOperation
    {
        public Mod Mod { get; }
        public Version VersionNumber { get; }

        public List<string> InstalledFiles { get; } = new();

        public Action<double, string> ProgressDialogueCallback { get; set; }

        public bool Completed { get; protected set; } = true;

        public string Message { get; protected set; }

        public ModOperation(Mod mod, Version versionNumber)
        {
            Mod = mod;
            VersionNumber = versionNumber;
        }

        public virtual Task Run()
        {
            SentrySdk.ConfigureScope(scope =>
            {
                scope.Contexts["mod"] = new
                {
                    Guid = Mod.Guid,
                    Version = VersionNumber.ToString()
                };
            });
            return Task.CompletedTask;
        }
    }
}
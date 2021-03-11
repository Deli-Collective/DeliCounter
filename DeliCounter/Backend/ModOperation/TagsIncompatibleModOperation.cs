using System;
using System.Threading.Tasks;
using Version = SemVer.Version;

namespace DeliCounter.Backend.ModOperation
{
    class TagsIncompatibleModOperation : ModOperation
    {
        public TagsIncompatibleModOperation(Mod mod, Version versionNumber) : base(mod, versionNumber)
        {
        }

        internal override Task Run()
        {
            Completed = false;
            Message = $"{Mod.Versions[VersionNumber].Name} can't be installed because it is incompatible with one or more of your installed mods. This probably means you have a mod which replaces or modifies the same item already installed.";
            return Task.CompletedTask;
        }
    }
}

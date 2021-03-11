using System;
using System.Linq;
using System.Threading.Tasks;
using Version = SemVer.Version;

namespace DeliCounter.Backend.ModOperation
{
    class TagsIncompatibleModOperation : ModOperation
    {
        private Mod[] _incompatibleMods;

        public TagsIncompatibleModOperation(Mod mod, Version versionNumber, Mod[] incompatible) : base(mod, versionNumber)
        {
            _incompatibleMods = incompatible;
        }

        internal override Task Run()
        {
            Completed = false;
            Message = $"{Mod.Versions[VersionNumber].Name} can't be installed because it is incompatible with one or more of your installed mods:\n" + string.Join('\n', _incompatibleMods.Select(x => x.Installed.Name));
            return Task.CompletedTask;
        }
    }
}

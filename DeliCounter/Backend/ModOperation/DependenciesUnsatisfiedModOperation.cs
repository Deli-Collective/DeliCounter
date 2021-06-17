using System.Threading.Tasks;
using Version = SemVer.Version;

namespace DeliCounter.Backend.ModOperation
{
    public class DependenciesUnsatisfiedModOperation : ModOperation
    {
        private readonly Mod _depMod;

        public DependenciesUnsatisfiedModOperation(Mod mod, Version versionNumber, Mod depMod) : base(mod, versionNumber)
        {
            _depMod = depMod;
        }

        public override Task Run()
        {
            Completed = false;
            var requestedVersion = Mod.Versions[VersionNumber].Dependencies[_depMod.Guid].MaxSatisfying(_depMod.Versions.Keys);
            Message = $"Could not satisfy a dependency of {Mod.Guid} because an incompatible version of {_depMod.Guid} is already installed. Try installing {_depMod.Guid} {requestedVersion}";
            return Task.CompletedTask;
        }
    }
}
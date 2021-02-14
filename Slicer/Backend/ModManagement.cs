using System.Collections.Generic;
using System.Linq;
using Slicer.Backend.ModOperation;
using Slicer.Controls;

namespace Slicer.Backend
{
    internal static class ModManagement
    {
        internal static void InstallMod(Mod mod)
        {
            App.RunInBackgroundThread(() =>
            {
                ExecuteOperations(EnumerateInstallDependencies(mod)
                    .Concat(new []{new InstallModOperation(mod)}));
            });
        }

        private static IEnumerable<ModOperation.ModOperation> EnumerateInstallDependencies(Mod mod)
        {
            var version = mod.Latest;
            foreach (var dep in version.Dependencies)
            {
                var depMod = ModRepository.Instance.Mods[dep.Key];
                foreach (var yield in EnumerateInstallDependencies(depMod))
                    yield return yield;
                if (!depMod.IsInstalled)
                    yield return new InstallModOperation(depMod);
                else if (depMod.InstalledVersion < dep.Value)
                {
                    yield return new UninstallModOperation(depMod);
                    yield return new InstallModOperation(depMod);
                }
            }
        }

        internal static void ExecuteOperations(IEnumerable<ModOperation.ModOperation> operations)
        {
            var ops = operations.ToArray();

            ProgressDialogue progressDialogue = null;
            App.RunInMainThread(() =>
            {
                progressDialogue = new ProgressDialogue { Title = "Executing operations" };
                progressDialogue.ShowAsync();
            });
            for (int i = 0; i < ops.Length; i++)
            {
                var op = ops[i];
                op.ProgressDialogueCallback = (progress, message) => App.RunInMainThread(() =>
                {
                    var oneOp = 1d / ops.Length;
                    progressDialogue.ProgressBar.Value = oneOp * i + oneOp * progress;
                    progressDialogue.Message.Text = message;
                });
                op.Run();
            }

            App.RunInMainThread(() =>
            {
                progressDialogue.Message.Text = $"{ops.Length} operation(s) completed successfully!";
                progressDialogue.ProgressBar.Value = 1;
                progressDialogue.IsPrimaryButtonEnabled = true;
            });
        }
    }
}
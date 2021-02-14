using System.Collections.Generic;
using System.Linq;
using ABI.Windows.ApplicationModel.AppExtensions;
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

        internal static void UninstallMod(Mod mod)
        {
            App.RunInBackgroundThread(() =>
            {
                ExecuteOperations(EnumerateUninstallDependencies(mod)
                    .Concat(new[] { new UninstallModOperation(mod) }));
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

        private static IEnumerable<ModOperation.ModOperation> EnumerateUninstallDependencies(Mod mod)
        {
            foreach (var dependent in ModRepository.Instance.Mods.Values.Where(m => m.IsInstalled))
            {
                if (dependent.Installed.Dependencies.Any(x => x.Key == mod.Guid))
                    yield return new UninstallModOperation(dependent);
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

            ModRepository.Instance.WriteCache();
        }
    }
}
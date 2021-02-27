using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DeliCounter;
using DeliCounter.Backend;
using DeliCounter.Backend.ModOperation;
using DeliCounter.Controls;
using DeliCounter.Properties;

namespace DeliCounter.Backend
{
    internal static class ModManagement
    {
        internal static void InstallMod(Mod mod)
        {
            App.RunInBackgroundThread(() =>
            {
                ExecuteOperations(EnumerateInstallDependencies(mod)
                    .Concat(new[] {new InstallModOperation(mod)}));
            });
        }

        internal static void UninstallMod(Mod mod)
        {
            App.RunInBackgroundThread(() =>
            {
                ExecuteOperations(EnumerateUninstallDependencies(mod)
                    .Concat(new[] {new UninstallModOperation(mod)}));
            });
        }

        internal static void UpdateMod(Mod mod)
        {
            App.RunInBackgroundThread(() =>
            {
                ExecuteOperations(new ModOperation.ModOperation[]
                {
                    new UninstallModOperation(mod),
                    new InstallModOperation(mod)
                });
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
                {
                    yield return new InstallModOperation(depMod);
                }
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
                if (dependent.Installed.Dependencies.Any(x => x.Key == mod.Guid))
                {
                    foreach (var yield in EnumerateUninstallDependencies(dependent))
                        yield return yield;
                    yield return new UninstallModOperation(dependent);
                }
        }

        /// <summary>
        ///     Executes a series of mod operations (uninstall, install)
        /// </summary>
        private static void ExecuteOperations(IEnumerable<ModOperation.ModOperation> operations)
        {
            if (Settings.Default.GameLocationOrError is null) return;

            var ops = operations.Distinct(new ModOperationEqualityComparer()).ToArray();

            ProgressDialogue progressDialogue = null;
            var error = false;
            var errIndex = -1;
            App.RunInMainThread(() =>
            {
                progressDialogue = new ProgressDialogue {Title = "Executing operations"};
                progressDialogue.ShowAsync();
            });
            for (var i = 0; i < ops.Length; i++)
            {
                var op = ops[i];
                op.ProgressDialogueCallback = (progress, message) => App.RunInMainThread(() =>
                {
                    var oneOp = 1d / ops.Length;
                    progressDialogue.ProgressBar.Value = oneOp * i + oneOp * progress;
                    progressDialogue.Message.Text = message;
                });

                try
                {
                    op.Run().GetAwaiter().GetResult();
                    if (!op.Completed) errIndex = i;
                }
                catch (Exception e)
                {
                    error = true;
                    DiagnosticInfoCollector.WriteExceptionToDisk(e);
                    break;
                }
            }

            App.RunInMainThread(() =>
            {
                progressDialogue.IsPrimaryButtonEnabled = true;
                progressDialogue.ProgressBar.Visibility = Visibility.Collapsed;
                if (error)
                {
                    progressDialogue.Title = "Error";
                    progressDialogue.Message.Text = "An exception has occured and the operation was not completed. An exception file was saved to the application's folder. Please submit it to the developers.";
                } else if (errIndex != -1)
                {
                    progressDialogue.Title = "Error";
                    progressDialogue.Message.Text = $"An operation did not complete successfully:\n{ops[errIndex].Message}";
                }
                else
                {
                    progressDialogue.Message.Text = $"{ops.Length} operation(s) completed successfully!";
                    progressDialogue.ProgressBar.Value = 1;
                }
            });

            ModRepository.Instance.WriteCache();
        }

        private class ModOperationEqualityComparer : IEqualityComparer<ModOperation.ModOperation>
        {
            public bool Equals(ModOperation.ModOperation x, ModOperation.ModOperation y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                return x.GetType() == y.GetType() && x.Mod.Equals(y.Mod);
            }

            public int GetHashCode(ModOperation.ModOperation obj)
            {
                return obj.Mod.GetHashCode();
            }
        }
    }
}
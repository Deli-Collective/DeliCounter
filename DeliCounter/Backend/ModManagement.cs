using DeliCounter.Backend.ModOperation;
using DeliCounter.Controls;
using DeliCounter.Properties;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Range = SemVer.Range;
using Version = SemVer.Version;

namespace DeliCounter.Backend
{
    internal static class ModManagement
    {
        internal static void InstallMod(Mod mod, Version versionNumber)
        {
            App.RunInBackgroundThread(() => { ExecuteOperations(EnumerateInstallDependencies(mod, versionNumber).Concat(new[] { new InstallModOperation(mod, versionNumber) })); });
        }

        internal static void InstallMods(IEnumerable<Mod> mods)
        {
            App.RunInBackgroundThread(() => {
                IEnumerable<ModOperation.ModOperation> enumerable = Array.Empty<ModOperation.ModOperation>();
                foreach (Mod mod in mods)
                    enumerable = enumerable.Concat(EnumerateInstallDependencies(mod, mod.LatestVersion).Concat(new[] { new InstallModOperation(mod, mod.LatestVersion) }));
                ExecuteOperations(enumerable);
            });
        }

        internal static void UninstallMods(IEnumerable<Mod> mods)
        {
            App.RunInBackgroundThread(() => {
                IEnumerable <ModOperation.ModOperation> enumerable = Array.Empty<ModOperation.ModOperation>();
                foreach (Mod mod in mods)
                    enumerable = enumerable.Concat(EnumerateUninstallDependencies(mod).Concat(new[] { new UninstallModOperation(mod) }));
                ExecuteOperations(enumerable);
            });
        }

        internal static void UpdateMod(Mod mod, Version versionNumber)
        {
            App.RunInBackgroundThread(() =>
            {
                List<ModOperation.ModOperation> ops = new();

                // Check that the requested version won't cause any issues
                var notSatisfied = new List<string>();
                foreach (var dependents in mod.InstalledDirectDependents)
                {
                    if (!dependents.Installed.Dependencies[mod.Guid].IsSatisfied(versionNumber))
                        notSatisfied.Add(dependents.ToString());
                }

                foreach ((string key, Range value) in mod.Versions[versionNumber].Dependencies)
                {
                    var dependency = ModRepository.Instance.Mods[key];

                    // If for some reason the user doesn't have a dependency installed, skip this because it will be installed later.
                    if (!dependency.IsInstalled) continue;
                        
                    if (!value.IsSatisfied(dependency.InstalledVersion))
                        notSatisfied.Add(dependency.ToString());
                }

                if (notSatisfied.Count == 0)
                    ExecuteOperations(EnumerateInstallDependencies(mod, versionNumber).Concat(new ModOperation.ModOperation[]
                    {
                        new UninstallModOperation(mod),
                        new InstallModOperation(mod, versionNumber)
                    }));
                else App.RunInMainThread(() =>
                {
                    string op = mod.InstalledVersion < versionNumber ? "updated" : "downgraded";
                    var dialogue = new AlertDialogue("Error", $"This mod cannot be {op} because one or more installed mods are not compatible with the selected version:\n" + string.Join(", ", notSatisfied));
                    App.Current.QueueDialog(dialogue);
                });
            });
        }

        internal static void UpdateMods(IEnumerable<Mod> mods)
        {
            foreach (Mod mod in mods) UpdateMod(mod, mod.LatestVersion);
        }

        internal static void DefaultAction(Mod mod)
        {
            // If the mod is not installed, install the latest version
            if (!mod.IsInstalled) InstallMod(mod, mod.LatestVersion);
            // If the mod is not up to date, update it.
            else if (!mod.UpToDate) UpdateMod(mod, mod.LatestVersion);
        }

        private static IEnumerable<ModOperation.ModOperation> EnumerateInstallDependencies(Mod mod, Version versionNumber)
        {
            var version = mod.Versions[versionNumber];

            var incompatible = version.IncompatibleInstalledMods.ToArray();
            if (incompatible.Length > 0)
            {
                yield return new TagsIncompatibleModOperation(mod, versionNumber, incompatible);
                yield break;
            }

            foreach (var (guid, compatibleRange) in version.Dependencies)
            {
                var depMod = ModRepository.Instance.Mods[guid];
                var depVersion = compatibleRange.MaxSatisfying(depMod.Versions.Keys);

                // There is no version that satisfies this?
                if (depVersion is null)
                {
                    SentrySdk.CaptureMessage($"User tries to install {mod.Guid} @ {versionNumber} but no valid version for dependency {depMod.Guid} was found!");
                    yield return new DependenciesUnsatisfiedModOperation(mod, versionNumber, depMod);
                    yield break;
                }

                foreach (var yield in EnumerateInstallDependencies(depMod, depVersion))
                    yield return yield;
                if (!depMod.IsInstalled)
                {
                    yield return new InstallModOperation(depMod, depVersion);
                }
                else if (!compatibleRange.IsSatisfied(depMod.InstalledVersion))
                {
                    yield return new DependenciesUnsatisfiedModOperation(mod, versionNumber, depMod);
                    yield break;
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
                progressDialogue = new ProgressDialogue { Title = "Executing operations" };
                App.Current.QueueDialog(progressDialogue);
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
                    if (!op.Completed)
                    {
                        errIndex = i;
                        break;
                    }
                }
                catch (Exception e)
                {
                    error = true;
                    SentrySdk.CaptureException(e);
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
                }
                else if (errIndex != -1)
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
                if (x is null) return false;
                if (y is null) return false;
                return x.GetType() == y.GetType() && x.Mod.Equals(y.Mod);
            }

            public int GetHashCode(ModOperation.ModOperation obj)
            {
                return obj.Mod.GetHashCode();
            }
        }
    }
}
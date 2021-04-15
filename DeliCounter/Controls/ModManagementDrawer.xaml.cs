using DeliCounter.Backend;
using ModernWpf.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Version = SemVer.Version;

namespace DeliCounter.Controls
{
    /// <summary>
    ///     Interaction logic for ModManagementDrawer.xaml
    /// </summary>
    public partial class ModManagementDrawer : UserControl
    {
        private List<Mod> _selectedMods = new();
        private Version _selectedVersion;

        public ModManagementDrawer()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        public void ClearSelected()
        {
            _selectedMods.Clear();
            SelectedUpdated();
        }

        public void AddSelected(IList selected)
        {
            foreach (ModListItem item in selected)
                _selectedMods.Add(item.Mod);
        }

        public void RemoveSelected(IList selected)
        {
            foreach (ModListItem item in selected)
                _selectedMods.Remove(item.Mod);
        }

        public void SelectedUpdated()
        {
            if (_selectedMods.Count == 1)
            {
                Mod mod = _selectedMods[0];
                if (mod is not null)
                {
                    _selectedVersion = mod.InstalledVersion ?? mod.LatestVersion;
                    ComboBoxVersion.IsEnabled = mod.Versions.Count > 1;
                    ComboBoxVersion.Visibility = Visibility.Visible;
                    ComboBoxVersion.ItemsSource = mod.Versions.Keys.OrderByDescending(x => x);
                    _selectedVersion = mod.Versions.Keys.Max();
                    ComboBoxVersion.SelectedItem = _selectedVersion;
                }
            }

            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (_selectedMods.Count == 0)
            {
                UpdateShowNone();
                return;
            } else if (_selectedMods.Count > 1)
            {
                UpdateShowMany();
                return;
            }

            Mod mod = _selectedMods[0];
            Mod.ModVersion version = mod.Versions[_selectedVersion ?? mod.LatestVersion];

            // Reset the button contents
            ButtonInstall.Content = "Install";
            ButtonUpdate.Content = "Update";
            ButtonUninstall.Content = "Uninstall";

            // Update text block visibility
            TextBlockDescriptionWrapper.Visibility = Visibility.Visible;
            TextBlockLatestWrapper.Visibility = Visibility.Visible;
            TextBlockInstalledWrapper.Visibility = Visibility.Visible;
            TextBlockAuthorsWrapper.Visibility = Visibility.Visible;
            TextBlockDependenciesWrapper.Visibility = Visibility.Visible;
            TextBlockSourceWrapper.Visibility = Visibility.Visible;

            // Update the text values
            TextBlockTitle.Text = version.Name;
            TextBlockDescription.Text = version.Description;
            TextBlockAuthors.Text = string.Join(", ", version.Authors);
            TextBlockLatest.Text = mod.LatestVersion.ToString();
            TextBlockInstalled.Text = mod.IsInstalled ? mod.Installed?.VersionNumber?.ToString() ?? "0.0.0" : "No";
            TextBlockDependencies.Text = string.Join(", ", version.Dependencies.Select(x => x.Key + " " + x.Value));
            TextBlockSource.Text = version.SourceUrl;
            HyperlinkSource.NavigateUri = !string.IsNullOrEmpty(version.SourceUrl) ? new Uri(version.SourceUrl, UriKind.Absolute) : null;

            // Update the action button visibility
            if (mod.IsInstalled)
            {
                ButtonInstall.IsEnabled = false;
                ButtonInstall.Visibility = Visibility.Collapsed;
                ButtonUninstall.IsEnabled = true;
                ButtonUninstall.Visibility = Visibility.Visible;
                ButtonUpdate.IsEnabled = !mod.UpToDate;
                ButtonUpdate.Visibility = mod.UpToDate ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                ButtonInstall.IsEnabled = true;
                ButtonInstall.Visibility = Visibility.Visible;
                ButtonUninstall.IsEnabled = false;
                ButtonUninstall.Visibility = Visibility.Collapsed;
                ButtonUpdate.IsEnabled = false;
                ButtonUpdate.Visibility = Visibility.Collapsed;
            }

            // Mod Preview Image
            if (string.IsNullOrEmpty(version.PreviewImageUrl))
                ModPreviewImage.Visibility = Visibility.Collapsed;
            else
            {
                ModPreviewImage.Visibility = Visibility.Visible;
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(version.IconUrl, UriKind.Absolute);
                bi.EndInit();
                ModPreviewImage.Source = bi;
            }

            UpdateUpdateButton();
        }

        private void UpdateShowNone()
        {
            // Hide all the text blocks
            TextBlockTitle.Text = "No mod selected";
            TextBlockDescriptionWrapper.Visibility = Visibility.Collapsed;
            TextBlockLatestWrapper.Visibility = Visibility.Collapsed;
            TextBlockInstalledWrapper.Visibility = Visibility.Collapsed;
            TextBlockAuthorsWrapper.Visibility = Visibility.Collapsed;
            TextBlockDependenciesWrapper.Visibility = Visibility.Collapsed;
            TextBlockSourceWrapper.Visibility = Visibility.Collapsed;

            // Enable and make the buttons visible
            ButtonInstall.IsEnabled = false;
            ButtonInstall.Visibility = Visibility.Collapsed;
            ButtonUpdate.IsEnabled = false;
            ButtonUpdate.Visibility = Visibility.Collapsed;
            ButtonUninstall.IsEnabled = false;
            ButtonUninstall.Visibility = Visibility.Collapsed;

            // Combobox
            ComboBoxVersion.IsEnabled = false;
            ComboBoxVersion.Visibility = Visibility.Collapsed;
            ComboBoxVersion.ItemsSource = null;

            // Preview Image
            ModPreviewImage.Visibility = Visibility.Collapsed;
        }

        private void UpdateShowMany()
        {
            // Hide all the text blocks
            TextBlockTitle.Text = $"{_selectedMods.Count} mods selected";
            TextBlockDescriptionWrapper.Visibility = Visibility.Collapsed;
            TextBlockLatestWrapper.Visibility = Visibility.Collapsed;
            TextBlockInstalledWrapper.Visibility = Visibility.Collapsed;
            TextBlockAuthorsWrapper.Visibility = Visibility.Collapsed;
            TextBlockDependenciesWrapper.Visibility = Visibility.Collapsed;
            TextBlockSourceWrapper.Visibility = Visibility.Collapsed;

            // Counts
            int installable = _selectedMods.Count(x => !x.IsInstalled);
            int updatable = _selectedMods.Count(x => !x.UpToDate && x.IsInstalled);
            int removable = _selectedMods.Count(x => x.IsInstalled);

            // Button text
            ButtonInstall.Content = $"Install {installable}";
            ButtonUpdate.Content = $"Update {updatable}";
            ButtonUninstall.Content = $"Uninstall {removable}";

            // Enable and make the buttons visible
            ButtonInstall.IsEnabled = installable > 0;
            ButtonInstall.Visibility = installable > 0 ? Visibility.Visible : Visibility.Collapsed;
            ButtonUpdate.IsEnabled = updatable > 0;
            ButtonUpdate.Visibility = updatable > 0 ? Visibility.Visible : Visibility.Collapsed;
            ButtonUninstall.IsEnabled = removable > 0;
            ButtonUninstall.Visibility = removable > 0 ? Visibility.Visible : Visibility.Collapsed;

            // Combobox
            ComboBoxVersion.IsEnabled = false;
            ComboBoxVersion.Visibility = Visibility.Collapsed;
            ComboBoxVersion.ItemsSource = null;

            // Preview Image
            ModPreviewImage.Visibility = Visibility.Collapsed;
        }

        private void HyperlinkSource_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var proc = new ProcessStartInfo("cmd", "/C start " + e.Uri.AbsoluteUri)
            {
                CreateNoWindow = true
            };
            Process.Start(proc);
            e.Handled = true;
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.SteamAppLocator.IsRunning)
            {
                App.Current.QueueDialog(new AlertDialogue("Cannot do this now", "Please close H3VR before modifying your install."));
                return;
            }

            if (_selectedMods.Count == 1)
                ModManagement.InstallMod(_selectedMods[0], _selectedVersion);
            else
                ModManagement.InstallMods(_selectedMods);
        }

        private async void ButtonUninstall_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.SteamAppLocator.IsRunning)
            {
                App.Current.QueueDialog(new AlertDialogue("Cannot do this now", "Please close H3VR before modifying your install."));
                return;
            }

            int dependentCount = _selectedMods.SelectMany(x => x.InstalledDependents).Distinct().Count();
            if (dependentCount > 0)
            {
                // Confirm they want to do this
                var alert = new AlertDialogue("Are you sure?", $"Removing the selected mods will also remove their dependencies! This will uninstall {dependentCount + 1} mods.")
                {
                    DefaultButton = ContentDialogButton.Primary,
                    PrimaryButtonText = "Ok",
                    SecondaryButtonText = "Cancel"
                };
                var result = await alert.ShowAsync();
                if (result != ContentDialogResult.Primary) return;
            }

            // Uninstall
            ModManagement.UninstallMods(_selectedMods.Where(x => x.IsInstalled));
        }

        private void ButtonUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            if (App.Current.SteamAppLocator.IsRunning)
            {
                App.Current.QueueDialog(new AlertDialogue("Cannot do this now", "Please close H3VR before modifying your install."));
                return;
            }

            if (_selectedMods.Count == 1)
                ModManagement.UpdateMod(_selectedMods[0], _selectedVersion);
            else
                ModManagement.UpdateMods(_selectedMods.Where(x => !x.UpToDate && x.IsInstalled));
        }

        private void ComboBoxVersion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedVersion = (Version)ComboBoxVersion.SelectedItem;
            UpdateDisplay();
        }

        private void UpdateUpdateButton()
        {
            if (_selectedMods.Count != 1) return;
            var mod = _selectedMods[0];
            if (!mod.IsInstalled) return;

            // Set the update button text to either update or downgrade depending on the selected version
            if (_selectedVersion == null) return;
            if (mod.InstalledVersion < _selectedVersion)
            {
                ButtonUpdate.Visibility = Visibility.Visible;
                ButtonUpdate.IsEnabled = true;
                ButtonUpdate.Content = "Update";
            }
            else if (mod.InstalledVersion > _selectedVersion)
            {
                ButtonUpdate.Visibility = Visibility.Visible;
                ButtonUpdate.IsEnabled = true;
                ButtonUpdate.Content = "Downgrade";
            }
            else
            {
                ButtonUpdate.Visibility = Visibility.Collapsed;
                ButtonUpdate.IsEnabled = false;
            }
        }
    }
}
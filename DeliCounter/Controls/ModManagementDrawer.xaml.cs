﻿using DeliCounter.Backend;
using ModernWpf.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Version = SemVer.Version;

namespace DeliCounter.Controls
{
    /// <summary>
    ///     Interaction logic for ModManagementDrawer.xaml
    /// </summary>
    public partial class ModManagementDrawer : UserControl
    {
        public Mod SelectedMod;
        private Version _selectedVersion;

        public ModManagementDrawer()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (SelectedMod == null)
            {
                UpdateShowNone();
                return;
            }

            // Get the selected mod and latest version
            var mod = SelectedMod;
            var version = mod.Latest;

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
            TextBlockLatest.Text = version.VersionNumber.ToString();
            TextBlockInstalled.Text = mod.IsInstalled ? mod.Installed.VersionNumber.ToString() : "No";
            TextBlockDependencies.Text = string.Join(", ", version.Dependencies.Select(x => x.Key + " " + x.Value));
            TextBlockSource.Text = version.SourceUrl;
            HyperlinkSource.NavigateUri = new Uri(version.SourceUrl, UriKind.Absolute);

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

            // Combobox
            ComboBoxVersion.IsEnabled = mod.Versions.Count > 1;
            ComboBoxVersion.Visibility = Visibility.Visible;
            ComboBoxVersion.ItemsSource = mod.Versions.Keys.OrderByDescending(x => x);
            _selectedVersion = mod.Versions.Keys.Max();
            ComboBoxVersion.SelectedItem = _selectedVersion;

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
            ModManagement.InstallMod(SelectedMod, _selectedVersion);
        }

        private async void ButtonUninstall_Click(object sender, RoutedEventArgs e)
        {
            var mod = SelectedMod;

            var dependentCount = mod.InstalledDependents.Count();
            if (dependentCount > 0)
            {
                // Confirm they want to do this
                var alert = new AlertDialogue("Are you sure?", $"You have {dependentCount} mod(s) installed which directly depend on this one, they will also be uninstalled. This operation is cascading.")
                {
                    DefaultButton = ContentDialogButton.Primary,
                    PrimaryButtonText = "Ok",
                    SecondaryButtonText = "Cancel"
                };
                var result = await alert.ShowAsync();
                if (result != ContentDialogResult.Primary) return;
            }

            // Uninstall
            ModManagement.UninstallMod(mod);
        }

        private void ButtonUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            ModManagement.UpdateMod(SelectedMod, _selectedVersion);
        }

        private void ComboBoxVersion_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedVersion = (Version)ComboBoxVersion.SelectedItem;
            UpdateUpdateButton();
        }

        private void UpdateUpdateButton()
        {
            var mod = SelectedMod;
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
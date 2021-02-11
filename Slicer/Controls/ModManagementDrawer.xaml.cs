using Slicer.Backend;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Slicer.Controls
{
    /// <summary>
    /// Interaction logic for ModManagementDrawer.xaml
    /// </summary>
    public partial class ModManagementDrawer : UserControl
    {
        public List<Mod> SelectedMods = new();

        public ModManagementDrawer()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (SelectedMods.Count != 1)
                UpdateShowMultiple();
            else UpdateShowOne();
        }

        private void UpdateShowMultiple()
        {
            // Hide all the text blocks
            TextBlockTitle.Text = $"{SelectedMods.Count} mods selected";
            TextBlockDescriptionWrapper.Visibility = Visibility.Collapsed;
            TextBlockLatestWrapper.Visibility = Visibility.Collapsed;
            TextBlockInstalledWrapper.Visibility = Visibility.Collapsed;
            TextBlockAuthorsWrapper.Visibility = Visibility.Collapsed;

            // Enable and make the buttons visible
            ButtonInstall.IsEnabled = true;
            ButtonInstall.Visibility = Visibility.Visible;
            ButtonUpdate.IsEnabled = true;
            ButtonUpdate.Visibility = Visibility.Visible;
            ButtonUninstall.IsEnabled = true;
            ButtonUninstall.Visibility = Visibility.Visible;
        }

        private void UpdateShowOne()
        {
            // Get the selected mod and latest version
            var mod = SelectedMods[0];
            var version = mod.Latest;

            // Update text block visibility
            TextBlockDescriptionWrapper.Visibility = Visibility.Visible;
            TextBlockLatestWrapper.Visibility = Visibility.Visible;
            TextBlockInstalledWrapper.Visibility = Visibility.Visible;
            TextBlockAuthorsWrapper.Visibility = Visibility.Visible;

            // Update the text values
            TextBlockTitle.Text = version.Name;
            TextBlockDescription.Text = version.Description;
            TextBlockAuthors.Text = string.Join(", ", version.Authors);
            TextBlockLatest.Text = version.VersionNumber.ToString();
            TextBlockInstalled.Text = mod.IsInstalled ? mod.Installed.VersionNumber.ToString() : "No";

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
        }
    }
}

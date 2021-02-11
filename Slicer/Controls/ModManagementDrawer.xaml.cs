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
        }

        public void UpdateDisplay()
        {
            if (SelectedMods.Count > 1)
                UpdateShowMultiple();
            else UpdateShowOne();
        }

        private void UpdateShowMultiple()
        {
            TextBlockTitle.Text = $"{SelectedMods.Count} mods selected";
            TextBlockDescriptionWrapper.Visibility = Visibility.Collapsed;
            TextBlockLatestWrapper.Visibility = Visibility.Collapsed;
            TextBlockInstalledWrapper.Visibility = Visibility.Collapsed;
            TextBlockAuthorsWrapper.Visibility = Visibility.Collapsed;
        }

        private void UpdateShowOne()
        {
            var mod = SelectedMods[0];
            var version = mod.Latest;


            TextBlockDescriptionWrapper.Visibility = Visibility.Visible;
            TextBlockLatestWrapper.Visibility = Visibility.Visible;
            TextBlockInstalledWrapper.Visibility = Visibility.Visible;
            TextBlockAuthorsWrapper.Visibility = Visibility.Visible;

            TextBlockTitle.Text = version.Name;
            TextBlockDescription.Text = version.Description;
            TextBlockAuthors.Text = string.Join(", ", version.Authors);
            TextBlockLatest.Text = version.VersionNumber.ToString();
            TextBlockInstalled.Text = mod.IsInstalled ? mod.Installed.VersionNumber.ToString() : "No";
        }
    }
}

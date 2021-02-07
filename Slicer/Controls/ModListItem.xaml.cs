using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Slicer.Backend;

namespace Slicer.Controls
{
    /// <summary>
    /// Interaction logic for ModListItem.xaml
    /// </summary>
    public partial class ModListItem
    {
        public ModListItem(Mod mod, bool displayInstalled = false)
        {
            InitializeComponent();

            // Get the version to display and edit the name and short description
            var version = displayInstalled ? mod.Installed : mod.Latest;
            ModTitle.Text = version.Name;
            ModShortDescription.Text = version.ShortDescription;

            // Update the local status icons (Up to date, update available)
            if (mod.IsInstalled)
            {
                LocalStatusIcon.Text = mod.UpToDate ? SegoeGlyphs.Checkmark : SegoeGlyphs.Repeat;
                LocalStatusIcon.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                LocalStatusIcon.Visibility = Visibility.Collapsed;
            }

            // Set the icon
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(version.IconUrl, UriKind.Relative);
            bi.EndInit();
            ModImage.Source = bi;
        }
    }
}
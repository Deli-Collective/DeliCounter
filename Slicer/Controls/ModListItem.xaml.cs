using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ModernWpf;
using Slicer.Backend;

namespace Slicer.Controls
{
    /// <summary>
    /// Interaction logic for ModListItem.xaml
    /// </summary>
    public partial class ModListItem
    {
        public Mod Mod;

        public ModListItem(Mod mod, bool displayInstalled = false)
        {
            Mod = mod;
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
                LocalStatusIcon.Text = SegoeGlyphs.Download;
                LocalStatusIcon.Style = (Style) Application.Current.Resources["BaseTextBlockStyle"];
            }

            // Set the icon
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(version.IconUrl, UriKind.Absolute);
            bi.EndInit();
            ModImage.Source = bi;
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
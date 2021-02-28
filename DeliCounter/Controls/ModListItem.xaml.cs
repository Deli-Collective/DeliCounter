using DeliCounter.Backend;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DeliCounter.Controls
{
    /// <summary>
    ///     Interaction logic for ModListItem.xaml
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

            // If this version has no info then actually do display the latest
            if (version.DownloadUrl is null) version = mod.Latest;

            ModTitle.Text = version.Name;
            ModShortDescription.Text = version.ShortDescription;

            // Set the icon
            if (version.IconUrl is not null)
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(version.IconUrl, UriKind.Absolute);
                bi.EndInit();
                ModImage.Source = bi;
            }

            // Update the local status icons (Up to date, update available)
            if (mod.IsInstalled)
            {
                LocalStatusIcon.Text = mod.UpToDate ? SegoeGlyphs.Checkmark : SegoeGlyphs.Repeat;
                LocalStatusIcon.Foreground = new SolidColorBrush(mod.UpToDate ? Colors.Green : Colors.DarkOrange);
            }
            else
            {
                LocalStatusIcon.Text = SegoeGlyphs.Download;
                LocalStatusIcon.Style = (Style)Application.Current.Resources["BaseTextBlockStyle"];
            }
        }
    }
}
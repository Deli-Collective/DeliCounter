using DeliCounter.Backend;
using DeliCounter.Controls.Abstract;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DeliCounter.Controls
{
    /// <summary>
    ///     Interaction logic for ModListItem.xaml
    /// </summary>
    public partial class CompactModListItem : ModListItem
    {
        public CompactModListItem(Mod mod) : base(mod)
        {
            InitializeComponent();

            // Get the version to display and edit the name and short description
            Mod.ModVersion version = mod.Latest;
            ModTitle.Text = version.Name;


            // Update the local status icons (Up to date, update available)
            if (mod.IsInstalled)
            {
                LocalStatusIcon.Text = mod.UpToDate ? SegoeGlyphs.Checkmark : SegoeGlyphs.Repeat;
                LocalStatusIcon.Foreground = new SolidColorBrush(mod.UpToDate ? Colors.Green : Colors.DarkOrange);
            }
            else if (version.IncompatibleInstalledMods.Any())
            {
                LocalStatusIcon.Text = "";
            }
            else
            {
                LocalStatusIcon.Text = SegoeGlyphs.Download;
                LocalStatusIcon.Style = (Style)Application.Current.Resources["BaseTextBlockStyle"];
            }
        }
    }
}
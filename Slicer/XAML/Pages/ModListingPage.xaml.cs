using Deli_Counter.Backend;
using Deli_Counter.Controls;
using Deli_Counter.XAML.Controls;
using Slicer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Deli_Counter.XAML.Pages
{
    /// <summary>
    /// Interaction logic for ModListingPage.xaml
    /// </summary>
    public partial class ModListingPage : ModernWpf.Controls.Page
    {
        public ModListingPage(ModCategory category)
        {
            InitializeComponent();

            PageTitle.Text = category.Name;

            foreach (var mod in ModRepository.Mods.Where(x => x.Category == category))
            {
                var latest = mod.LatestVersion;

                var card = new ModDisplayCard();
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(latest.IconUrl, UriKind.Absolute);
                bitmap.EndInit();
                card.ModImage.Source = bitmap;
                card.ModShortDescription.Text = latest.ShortDescription;
                card.ModTitle.Text = latest.Name;

                ModList.Items.Add(card);
            }
        }
    }
}

using Deli_Counter.XAML.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Deli_Counter.XAML.Pages
{
    /// <summary>
    /// Interaction logic for InstalledModsPage.xaml
    /// </summary>
    public partial class InstalledModsPage : ModernWpf.Controls.Page
    {
        public List<object> Mods;

        public InstalledModsPage()
        {
            InitializeComponent();

            var card = new ModDisplayCard();
            var fullFilePath = @"http://www.americanlayout.com/wp/wp-content/uploads/2012/08/C-To-Go-300x300.png";

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            bitmap.EndInit();

            card.ModImage.Source = bitmap;

            ModList.Items.Add(card);
        }
    }
}

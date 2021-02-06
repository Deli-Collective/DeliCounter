using System.Collections.Generic;
using System.Linq;
using ModernWpf.Controls;
using Slicer.Pages;

namespace Slicer
{
    public partial class MainWindow
    {
        private readonly Dictionary<string, object> _pages = new()
        {
            ["home"] = new HomePage(),
            ["installed"] = null,
            ["settings"] = new SettingsPage(),
        };

        public MainWindow()
        {
            InitializeComponent();
            NavView.SelectedItem = NavView.MenuItems[1];
            NavViewContent.Navigate(_pages["home"]);
        }

        private void NavView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var tag = args.IsSettingsInvoked ? "settings" : args.InvokedItemContainer.Tag.ToString();
            NavViewContent.Navigate(tag != null ? _pages[tag] : null);
        }
    }
}

using System.Collections.Generic;
using ModernWpf.Controls;
using Slicer.Pages;

namespace Slicer
{
    public partial class MainWindow
    {
        private readonly Dictionary<string, object> _pages = new()
        {
            ["home"] = null,
            ["installed"] = null,
            ["settings"] = new SettingsPage(),
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var tag = args.IsSettingsInvoked ? "settings" : args.InvokedItemContainer.Tag.ToString();
            NavViewContent.Navigate(tag != null ? _pages[tag] : null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Deli_Counter.XAML;
using ModernWpf.Controls;

namespace Slicer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set the page to the home page when we start
            NavView.SelectedItem = NavView.MenuItems[0];
            ContentFrame.Navigate(_pages["home"]);
        }

        /// <summary>
        ///     This is a dictionary of the pages accessible from the navigation menu
        /// </summary>
        private readonly Dictionary<string, object> _pages = new()
        {
            ["home"] = new HomePage(),
            ["installed"] = null,
            ["settings"] = null,
            ["mods"] = null,
            ["not_found"] = null
        };
        
        /// <summary>
        ///     Called when the user clicks (invokes) an item in the nav menu
        /// </summary>
        private void NavView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            // Get the tag that was clicked and navigate the content frame to the correct screen
            var tag = args.IsSettingsInvoked ? "settings" : args.InvokedItemContainer.Tag.ToString();
            ContentFrame.Navigate(_pages[tag ?? "not_found"]);
        }
    }
}
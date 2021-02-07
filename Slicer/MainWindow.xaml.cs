using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ModernWpf.Controls;
using Slicer.Backend;
using Slicer.Pages;

namespace Slicer
{
    public partial class MainWindow
    {
        private readonly Dictionary<string, UIElement> _pages = new()
        {
            ["home"] = new HomePage(),
            ["settings"] = new SettingsPage()
        };

        public MainWindow()
        {
            InitializeComponent();
            NavView.SelectedItem = NavView.MenuItems[0];
            NavViewContent.Navigate(_pages["home"]);
            ModRepository.Instance.RepositoryUpdated += ModRepoUpdated;
        }

        private void ModRepoUpdated(ModRepository.State state, Exception e)
        {
            // Remove the existing mod tabs
            var toRemove = from NavigationViewItemBase navViewMenuItem in NavView.MenuItems
                let foo = navViewMenuItem.Tag.ToString()?.StartsWith("mods") where foo.HasValue && foo.Value select navViewMenuItem;
            foreach (var remove in toRemove)
            {
                NavView.MenuItems.Remove(remove);
                _pages.Remove(remove.Tag.ToString() ?? string.Empty);
            }

            // Add new ones for each category (Including a local one)
            void AddCategory(ModCategory category)
            {
                NavView.MenuItems.Add(new NavigationViewItem
                {
                    Tag = "mods" + category.Path,
                    Content = category.Name,
                    Icon = new FontIcon
                    {
                        FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"),
                        Glyph = category.Icon
                    }
                });
                _pages.Add("mods" + category.Path, new ModListing(category));
            }
            
            AddCategory(new ModCategory
            {
                IsLocal = true,
                Name = "Installed Mods",
                Description = "List of all your installed mods",
                Icon = SegoeGlyphs.Save,
                Path = "Installed"
            });
            foreach (var category in ModRepository.Instance.Categories) AddCategory(category);
        }

        private void NavView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var tag = args.IsSettingsInvoked ? "settings" : args.InvokedItemContainer.Tag.ToString();
            NavViewContent.Navigate(tag != null ? _pages[tag] : null);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ModRepository.Instance.Refresh();
        }

        private void NavView_PaneToggled(NavigationView sender, object args)
        {
            if (NavView.IsPaneOpen)
                DownloadableHeader.Visibility = Visibility.Visible;
            else DownloadableHeader.Visibility = Visibility.Collapsed;
        }
    }
}
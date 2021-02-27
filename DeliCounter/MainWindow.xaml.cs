using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using DeliCounter.Backend;
using DeliCounter.Pages;
using DeliCounter.Properties;
using ModernWpf;
using ModernWpf.Controls;

namespace DeliCounter
{
    public partial class MainWindow
    {
        public string CurrentPage { get; private set; }

        private readonly Dictionary<string, (UIElement, bool)> _pages = new()
        {
            ["home"] = (new HomePage(), false),
            ["settings"] = (new SettingsPage(), false),
            ["search"] = (new SearchPage(), true)
        };

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            NavView.SelectedItem = NavView.MenuItems[0];
            NavViewContent.Navigate(_pages["home"].Item1);
            CurrentPage = "home";
            ModRepository.Instance.RepositoryUpdated += ModRepoUpdated;
        }

        public static MainWindow Instance { get; set; }

        private void ModRepoUpdated()
        {
            App.RunInMainThread(() =>
            {
                try
                {
                    // Remove the existing mod tabs
                    for (int i = NavView.MenuItems.Count - 1; i >= 4; i--)
                    {
                        NavView.MenuItems.RemoveAt(i);
                    }
                }
                catch (Exception e)
                {
                    // Ignored
                }


                var toRemove = _pages.Keys.Where(x => x.StartsWith("mods")).ToArray();
                foreach (var remove in toRemove) _pages.Remove(remove);

                // Add new ones for each category (Including a local one)
                void AddCategory(ModCategory category)
                {
                    NavView.MenuItems.Add(new NavigationViewItem
                    {
                        Tag = "mods" + category.Path,
                        Content = category.Name,
                        Icon = new FontIcon
                        {
                            FontFamily = new FontFamily("Segoe MDL2 Assets"),
                            Glyph = category.Icon
                        }
                    });
                    _pages.Add("mods" + category.Path, (new ModListing(category), true));
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
            });
        }

        private void NavView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var tag = args.IsSettingsInvoked ? "settings" : args.InvokedItemContainer.Tag.ToString();
            CurrentPage = tag;
            var page = _pages[tag].Item1;
            NavViewContent.Navigate(page);
            Drawer.IsPaneOpen = _pages[tag].Item2;

            if (page is ModListing listPage)
                listPage.ModList.SelectedItem = null;

            ModManagementDrawer.SelectedMods.Clear();
            ModManagementDrawer.UpdateDisplay();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem((_) => ModRepository.Instance.Refresh());
        }

        private void NavView_PaneToggled(NavigationView sender, object args)
        {
            DownloadableHeader.Visibility = NavView.IsPaneOpen ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
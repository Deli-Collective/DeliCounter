using DeliCounter.Backend;
using DeliCounter.Controls;
using DeliCounter.Controls.Abstract;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DeliCounter.Pages
{
    public partial class ModListing
    {
        private readonly ModCategory _category;

        public ModListing(ModCategory category)
        {
            InitializeComponent();
            ModRepository.Instance.InstalledModsUpdated += () => App.RunInMainThread(Update);
            _category = category;
            CategoryTitle.Text = category.Name;
            CategoryDescription.Text = category.Description;
            Update();
        }

        public void Update()
        {
            ModList.Items.Clear();
            foreach (var mod in _category.Mods)
                ModList.Items.Add(App.Current.Settings.ModListItemType switch
                {
                    ModListItemType.LargeWithIcon => new LargeModListItem(mod, true),
                    ModListItemType.Large => new LargeModListItem(mod, false),
                    _ => new CompactModListItem(mod)
                });
        }

        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var drawer = MainWindow.Instance.ModManagementDrawer;
            drawer.AddSelected(e.AddedItems);
            drawer.RemoveSelected(e.RemovedItems);
            drawer.SelectedUpdated();
        }

        private void ModItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (App.Current.SteamAppLocator.IsRunning)
            {
                App.Current.QueueDialog(new AlertDialogue("Cannot do this now", "Please close H3VR before modifying your install."));
                return;
            }

            ModListItem source = (ModListItem) ((ListViewItem) sender).Content;
            ModManagement.DefaultAction(source.Mod);
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class ListWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (targetType != typeof(double))
                throw new InvalidOperationException("The target must be a double");

            return Math.Max((double)value - 2, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
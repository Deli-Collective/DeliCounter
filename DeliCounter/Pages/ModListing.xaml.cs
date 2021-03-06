using DeliCounter.Backend;
using DeliCounter.Controls;
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

        private void Update()
        {
            ModList.Items.Clear();
            foreach (var mod in _category.Mods)
                ModList.Items.Add(new ModListItem(mod, mod.IsInstalled));
        }

        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var drawer = MainWindow.Instance.ModManagementDrawer;
            drawer.SelectedMod = ((ModListItem)ModList.SelectedItem)?.Mod;
            drawer.UpdateDisplay();
        }

        private void ModItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
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
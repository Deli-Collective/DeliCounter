using DeliCounter.Backend;
using DeliCounter.Controls;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeliCounter.Pages
{
    public partial class SearchPage
    {
        public SearchPage()
        {
            InitializeComponent();
            ModRepository.Instance.InstalledModsUpdated += Instance_InstalledModsUpdated;
        }

        private void Instance_InstalledModsUpdated()
        {
            App.RunInMainThread(() => UpdateSearch(TextBoxSearch.Text.ToLower()));

        }

        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var drawer = MainWindow.Instance.ModManagementDrawer;
            drawer.AddSelected(e.AddedItems);
            drawer.RemoveSelected(e.RemovedItems);
            drawer.SelectedUpdated();
        }

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var query = TextBoxSearch.Text.ToLower();
            UpdateSearch(query);
        }

        private void UpdateSearch(string query)
        {
            ModList.Items.Clear();
            if (string.IsNullOrEmpty(query)) return;
            foreach (var mod in ModRepository.Instance.Mods.Values.Where(x => x.MatchesQuery(query)))
                ModList.Items.Add(new ModListItem(mod, mod.IsInstalled));
        }

        private void ModItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (App.Current.SteamAppLocator.IsRunning)
            {
                App.Current.QueueDialog(new AlertDialogue("Cannot do this now", "Please close H3VR before modifying your install."));
                return;
            }

            ModListItem source = (ModListItem)((ListViewItem)sender).Content;
            ModManagement.DefaultAction(source.Mod);
        }
    }
}
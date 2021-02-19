using System.Linq;
using System.Windows.Controls;
using DeliCounter.Backend;
using DeliCounter.Controls;

namespace DeliCounter.Pages
{
    public partial class SearchPage
    {
        public SearchPage()
        {
            InitializeComponent();
        }
        
        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var drawer = MainWindow.Instance.ModManagementDrawer;
            drawer.SelectedMods.AddRange(e.AddedItems.Cast<ModListItem>().Select(x => x.Mod));
            foreach (var mod in e.RemovedItems.Cast<ModListItem>().Select(x => x.Mod))
                drawer.SelectedMods.Remove(mod);
            drawer.UpdateDisplay();
        }

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var query = TextBoxSearch.Text.ToLower();
            ModList.Items.Clear();
            if (string.IsNullOrEmpty(query)) return;
            foreach (var mod in ModRepository.Instance.Mods.Values.Where(x => x.MatchesQuery(query)))
                ModList.Items.Add(new ModListItem(mod, mod.IsInstalled));
        }
    }
}
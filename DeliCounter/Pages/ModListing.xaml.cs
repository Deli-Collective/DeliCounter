using System.Linq;
using System.Windows.Controls;
using DeliCounter.Backend;
using DeliCounter.Controls;

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
            drawer.SelectedMod = ((ModListItem) ModList.SelectedItem)?.Mod;
            drawer.UpdateDisplay();
        }
    }
}
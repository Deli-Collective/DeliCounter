using System.Linq;
using System.Windows.Controls;
using Slicer.Backend;
using Slicer.Controls;

namespace Slicer.Pages
{
    public partial class ModListing
    {
        private readonly ModCategory _category;

        public ModListing(ModCategory category)
        {
            InitializeComponent();
            ModRepository.Instance.RepositoryUpdated += () => App.RunInMainThread(Update);
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
            drawer.SelectedMods.AddRange(e.AddedItems.Cast<ModListItem>().Select(x => x.Mod));
            foreach (var mod in e.RemovedItems.Cast<ModListItem>().Select(x => x.Mod))
                drawer.SelectedMods.Remove(mod);
            drawer.UpdateDisplay();
        }
    }
}
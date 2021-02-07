using Slicer.Backend;
using Slicer.Controls;

namespace Slicer.Pages
{
    public partial class ModListing
    {
        private ModCategory _category;
        public ModListing(ModCategory category)
        {
            InitializeComponent();
            ModRepository.Instance.RepositoryUpdated += (state, exception) => Update();
            _category = category;
        }

        private void Update()
        {
            ModList.Children.Clear();
            foreach (var mod in ModRepository.Instance.Mods.Values)
                ModList.Children.Add(new ModListItem(mod, mod.IsInstalled));
        }
    }
}
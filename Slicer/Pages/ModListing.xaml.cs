using System.Windows;
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
    }
}
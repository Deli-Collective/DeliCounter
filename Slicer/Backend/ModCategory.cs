using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slicer.Backend
{
    public class ModCategory
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public bool IsLocal { get; set; } = false;

        public IEnumerable<Mod> Mods => IsLocal
            ? ModRepository.Instance.Mods.Values.Where(m => m.IsInstalled)
            : ModRepository.Instance.Mods.Values.Where(m => m.Category == this);
    }
}

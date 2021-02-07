using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slicer.Backend
{
    class ModCategory
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public IEnumerable<Mod> Mods { get; set; }
    }
}

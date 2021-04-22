using DeliCounter.Backend;
using System.Windows.Controls;

namespace DeliCounter.Controls.Abstract
{
    public enum ModListItemType
    {
        LargeWithIcon,
        Large,
        Compact
    }

    public class ModListItem : UserControl
    {
        public Mod Mod { get; private set; }

        public ModListItem(Mod mod)
        {
            Mod = mod;
        }
    }
}

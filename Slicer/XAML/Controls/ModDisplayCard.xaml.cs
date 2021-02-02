using Deli_Counter.Backend;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Deli_Counter.XAML.Controls
{
    /// <summary>
    /// Interaction logic for ModDisplayCard.xaml
    /// </summary>
    public partial class ModDisplayCard : UserControl
    {
        //public static readonly DependencyProperty ModProperty = DependencyProperty.Register("Mod", typeof(Mod), null);

        /*
        public Mod Mod
        {
            get { return (Mod)GetValue(ModProperty); }
            set { SetValue(ModProperty, value); }
        }
        */

        public ModDisplayCard()
        {
            InitializeComponent();
        }
    }
}

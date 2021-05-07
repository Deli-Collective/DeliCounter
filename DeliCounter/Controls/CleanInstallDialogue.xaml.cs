using DeliCounter.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeliCounter.Controls
{
    /// <summary>
    /// Interaction logic for CleanInstallDialogue.xaml
    /// </summary>
    public partial class CleanInstallDialogue
    {
        public CleanInstallDialogue()
        {
            InitializeComponent();
        }

        private void ContentDialog_SecondaryButtonClick(ModernWpf.Controls.ContentDialog sender, ModernWpf.Controls.ContentDialogButtonClickEventArgs args)
        {
            App.Current.DiagnosticInfoCollector.CleanInstallFolder(App.Current.Settings.GameLocation);
        }

        private void ContentDialog_PrimaryButtonClick(ModernWpf.Controls.ContentDialog sender, ModernWpf.Controls.ContentDialogButtonClickEventArgs args)
        {
            
        }
    }
}

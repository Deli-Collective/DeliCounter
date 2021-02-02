using Deli_Counter.Properties;
using ModernWpf;
using ModernWpf.Controls;
using Slicer;
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

namespace Deli_Counter.XAML.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsPage : ModernWpf.Controls.Page
    {
        private bool _initializing = true;

        public Flyout? SaveFlyout;

        public SettingsPage()
        {
            InitializeComponent();
            SaveFlyout = FlyoutService.GetFlyout(SaveButton) as Flyout;
            _initializing = false;
            LoadSettings();

            LostFocus += SettingsPage_LostFocus;
        }

        private void SettingsPage_LostFocus(object sender, RoutedEventArgs e)
        {
            // TODO: This causes issues with the password box
            //SaveSettings();
        }

        private void AutoDetectGameLocation_Checked(object sender, RoutedEventArgs e)
        {
            // If this is called before InitializeComponent is completed
            if (_initializing) return;

            if (AutoDetectGameLocation.IsChecked!.Value)
            {
                // TODO: Auto detect here
                GameLocation.Text = "(Auto-detected)";
                GameLocation.IsEnabled = false;
            }
            else
            {
                GameLocation.Text = "";
                GameLocation.IsEnabled = true;
            }
        }

        private void DarkMode_Checked(object sender, RoutedEventArgs e)
        {
            if (DarkMode.IsChecked!.Value)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            } else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
        }

        private void GitAnonymous_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing) return;

            if (GitAnonymous.IsChecked!.Value)
            {
                GitUsername.IsEnabled = false;
                GitPassword.IsEnabled = false;
            } else
            {
                GitUsername.IsEnabled = true;
                GitPassword.IsEnabled = true;
            }
        }

        private void LoadSettings()
        {
            var settings = Settings.Default;

            AutoDetectGameLocation.IsChecked = settings.AutoDetectGameLocation;
            if (!settings.AutoDetectGameLocation) GameLocation.Text = settings.GameLocation;
            DarkMode.IsChecked = settings.DarkMode;

            GitRepo.Text = settings.GitRepository;
            GitAnonymous.IsChecked = settings.GitAnonymous;
            if (!settings.GitAnonymous)
            {
                GitUsername.Text = settings.GitUsername;
                GitPassword.Password = settings.GitPassword;
            }
        }

        private void SaveSettings()
        {
            var settings = Settings.Default;

            settings.AutoDetectGameLocation = AutoDetectGameLocation.IsChecked!.Value;
            settings.GameLocation = GameLocation.Text;
            settings.DarkMode = DarkMode.IsChecked!.Value;

            settings.GitRepository = GitRepo.Text;
            settings.GitAnonymous = GitAnonymous.IsChecked!.Value;
            settings.GitUsername = GitUsername.Text;
            settings.GitPassword = GitPassword.Password;

            settings.Save();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }
    }
}

using System.ComponentModel;
using System.Configuration;
using ModernWpf;

namespace Slicer.Properties
{
    internal sealed partial class Settings
    {
        public Settings()
        {
            PropertyChanged += Settings_PropertyChanged;
            SettingsLoaded += Settings_SettingsLoaded;
        }

        private void Settings_SettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            ThemeManager.Current.ApplicationTheme = EnableDarkMode ? ApplicationTheme.Dark : ApplicationTheme.Light;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Save();
        }
    }
}
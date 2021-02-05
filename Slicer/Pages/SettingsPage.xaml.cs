using System;
using System.Windows;
using System.Windows.Data;
using Deli_Counter.Backend;
using ModernWpf;
using Slicer.Properties;

namespace Slicer.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage
    {
        private Settings _settings;

        public SettingsPage()
        {
            InitializeComponent();
            _settings = Settings.Default;
            DataContext = _settings;
        }

        private void AutoDetectGameLocation_OnChecked(object sender, RoutedEventArgs e)
        {
            _settings.GameLocation = Utilities.GameDirectory;

            // If it wasn't set revert since it can't be found automatically
            if (string.IsNullOrEmpty(_settings.GameLocation)) _settings.AutoDetectGameLocation = false;
        }

        private void DarkMode_OnChecked(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ApplicationTheme = _settings.EnableDarkMode ? ApplicationTheme.Dark : ApplicationTheme.Light;
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

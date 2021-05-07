using Bluegrams.Application;
using DeliCounter.Backend;
using DeliCounter.Controls;
using DeliCounter.Properties;
using ModernWpf;
using ModernWpf.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace DeliCounter
{
    public partial class App
    {
        public App()
        {
            // Configure some things
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new SemRangeConverter(), new SemVersionConverter() }
            };
            Settings = Settings.Default;
            SteamAppLocator = new SteamAppLocator(450540, "H3VR", "h3vr.exe");
            DiagnosticInfoCollector = new DiagnosticInfoCollector(SteamAppLocator);

            // Setup the settings file stuff
            PortableJsonSettingsProvider.SettingsDirectory = SteamAppLocator.AppLocation;
            PortableJsonSettingsProvider.SettingsFileName = "DeliCounter.cfg";
            PortableJsonSettingsProvider.ApplyProvider(Settings);

            // Initialize Sentry
            DiagnosticInfoCollector.InitSentry();

            // Check if Windows wants light or dark theme
            var uiSettings = new UISettings();
            var color = uiSettings.GetColorValue(UIColorType.Background);
            bool darkModeRequestedByOS = color == Color.FromArgb(255, 0, 0, 0);
            
            // If this is our first run, honor the OS settings, otherwise keep what is in the settings
            if (Settings.FirstRun) Settings.EnableDarkMode = darkModeRequestedByOS;
            ThemeManager.Current.ApplicationTheme = Settings.EnableDarkMode ? ApplicationTheme.Dark : ApplicationTheme.Light;

        }

        public SteamAppLocator SteamAppLocator { get; }

        public IDisposable SentryDisposable { get; }

        public DiagnosticInfoCollector DiagnosticInfoCollector { get; }

        internal Settings Settings { get; }

        public static void RunInBackgroundThread(Action action)
        {
            ThreadPool.QueueUserWorkItem(_ => action());
        }

        public static void RunInMainThread(Action action)
        {
            Current.Dispatcher.Invoke(action);
        }

        public new static App Current => (App)Application.Current;

        private void App_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            if (Settings.FirstRun)
            {
                var dialogue = new AlertDialogue("Disclaimer", "You are about to mod your game. By continuing, you acknowledge that you may encounter issues and that these issues are NOT to be reported to the developer of the game. Please instead report all issues to the mod authors who can be contacted via their mod's source URL or in the main H3 Discord.");
                QueueDialog(dialogue);
                var secondDialogue = new CleanInstallDialogue();
                QueueDialog(secondDialogue);
                Settings.FirstRun = false;
            }
        }

        private Queue<ContentDialog> _contentDialogQueue = new();

        public async void QueueDialog(ContentDialog dialog)
        {
            _contentDialogQueue.Enqueue(dialog);
            if (_contentDialogQueue.Count == 1)
            {
                while (_contentDialogQueue.Count > 0)
                {
                    try
                    {
                        await _contentDialogQueue.Peek().ShowAsync();
                    } catch (InvalidOperationException)
                    {
                        // Not sure what causes this but just ignore it
                    }
                    _contentDialogQueue.Dequeue();
                }
            }
        }

        public bool AreDialogsQueued => _contentDialogQueue.Count > 0;
    }
}
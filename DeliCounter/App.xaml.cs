using DeliCounter.Backend;
using DeliCounter.Controls;
using DeliCounter.Properties;
using ModernWpf;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;

namespace DeliCounter
{
    public partial class App
    {
        public App()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new SemRangeConverter(), new SemVersionConverter() }
            };

            // Init Sentry
            SentryDisposable = SentrySdk.Init(o => {
                o.Dsn = "https://bcfd3132000f420986e6c816e9d8a621@o567748.ingest.sentry.io/5712026";
                o.ShutdownTimeout = TimeSpan.FromSeconds(5);
            });

            ThemeManager.Current.ApplicationTheme = Settings.Default.EnableDarkMode ? ApplicationTheme.Dark : ApplicationTheme.Light;
            SteamAppLocator = new SteamAppLocator(450540, "H3VR", "h3vr.exe");
            DiagnosticInfoCollector = new DiagnosticInfoCollector(SteamAppLocator);

            throw new Exception("Test!");
        }

        public SteamAppLocator SteamAppLocator { get; }

        public IDisposable SentryDisposable { get; }

        public DiagnosticInfoCollector DiagnosticInfoCollector { get; }

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
            if (Settings.Default.FirstRun)
            {
                var dialogue = new AlertDialogue("Disclaimer", "You are about to mod your game. By continuing, you acknowledge that you may encounter issues and that these issues are NOT to be reported to the developer of the game. Please instead report all issues to the mod authors who can be contacted via their mod's source URL or in the main H3 Discord.");
                dialogue.ShowAsync();
                Settings.Default.FirstRun = false;
            }
        }
    }
}
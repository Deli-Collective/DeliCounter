using DeliCounter.Backend;
using DeliCounter.Controls;
using DeliCounter.Properties;
using ModernWpf;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

            ThemeManager.Current.ApplicationTheme = Settings.Default.EnableDarkMode ? ApplicationTheme.Dark : ApplicationTheme.Light;
            SteamAppLocator = new SteamAppLocator(450540, "H3VR", "h3vr.exe");
            DiagnosticInfoCollector = new DiagnosticInfoCollector(SteamAppLocator);

            DiagnosticInfoCollector.InitSentry();
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
                App.Current.QueueDialog(dialogue);
                Settings.Default.FirstRun = false;
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
                    await _contentDialogQueue.Peek().ShowAsync();
                    _contentDialogQueue.Dequeue();
                }
            }
        }
    }
}
using System;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using DeliCounter.Backend;
using DeliCounter.Controls;
using DeliCounter.Properties;
using ModernWpf.Controls;

namespace DeliCounter
{
    public partial class App
    {
        public App()
        {
            SteamAppLocator = new SteamAppLocator(0, "H3VR", "h3vr.exe");
            DiagnosticInfoCollector = new DiagnosticInfoCollector(SteamAppLocator);
        }
        
        public SteamAppLocator SteamAppLocator { get; }
        
        public DiagnosticInfoCollector DiagnosticInfoCollector { get; }

        public static void RunInBackgroundThread(Action action) => ThreadPool.QueueUserWorkItem(_ => action());

        public static void RunInMainThread(Action action) => Current.Dispatcher.Invoke(action);

        public static new App Current => (App) Application.Current;

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
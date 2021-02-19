using System;
using System.Threading;
using System.Windows;
using DeliCounter.Backend;

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
    }
}
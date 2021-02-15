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
            AppDomain.CurrentDomain.UnhandledException += InfoCollector.CurrentDomainOnUnhandledException;
        }

        public static void RunInBackgroundThread(Action action) => ThreadPool.QueueUserWorkItem((_) => action());

        public static void RunInMainThread(Action action) => Application.Current.Dispatcher.Invoke(action);
    }
}
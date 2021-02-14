using System;
using System.Threading;
using System.Windows;

namespace Slicer
{
    public partial class App
    {
        public static void RunInBackgroundThread(Action action) => ThreadPool.QueueUserWorkItem((_) => action());

        public static void RunInMainThread(Action action) => Application.Current.Dispatcher.Invoke(action);
    }
}
using DeliCounter.Controls;
using System;
using System.ComponentModel;
using System.Windows;

namespace DeliCounter.Properties
{
    internal sealed partial class Settings
    {
        public Settings()
        {
            PropertyChanged += Settings_PropertyChanged;
        }

        /// <summary>
        ///     Returns the game location or null if it is missing.
        ///     Also displays a dialogue if it is null
        /// </summary>
        public string GameLocationOrError
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(GameLocation)) return GameLocation;

                App.RunInMainThread(() =>
                {
                    var mainWindow = (MainWindow)((App)Application.Current).MainWindow;
                    if (mainWindow.CurrentPage != "settings")
                        App.Current.QueueDialog(new AlertDialogue("Game location missing",
                            "Your game location is not set! Please set it in the settings menu."));
                });
                return null;
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameLocation))
                GameLocationChanged?.Invoke();
            Save();
        }

        public event Action GameLocationChanged;
    }
}
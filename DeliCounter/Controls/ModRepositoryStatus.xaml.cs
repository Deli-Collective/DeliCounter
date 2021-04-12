using DeliCounter.Backend;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace DeliCounter.Controls
{
    /// <summary>
    ///     Interaction logic for RepositoryStatus.xaml
    /// </summary>
    public partial class ModRepositoryStatus : UserControl
    {
        public ModRepositoryStatus()
        {
            InitializeComponent();
            ModRepository.Instance.RepositoryUpdated += Update;
        }

        private void Update()
        {
            App.RunInMainThread(() =>
            {
                ButtonRefresh.IsEnabled = true;
                switch (ModRepository.Instance.Status)
                {
                    case ModRepository.State.Error:
                        StatusIcon.Text = SegoeGlyphs.X;
                        StatusIcon.Foreground = new SolidColorBrush(Colors.Red);
                        LastUpdateText.Text = ModRepository.Instance.Exception.Message;
                        StatusText.Text = "Error";
                        ButtonReset.IsEnabled = true;
                        break;
                    case ModRepository.State.CantUpdate:
                        StatusIcon.Text = "\uF13C";
                        StatusIcon.Foreground = new SolidColorBrush(Colors.Orange);
                        LastUpdateText.Text = ModRepository.Instance.Exception.Message;
                        StatusText.Text = "Offline";
                        ButtonReset.IsEnabled = true;
                        break;
                    case ModRepository.State.UpToDate:
                        StatusIcon.Text = SegoeGlyphs.Checkmark;
                        StatusIcon.Foreground = new SolidColorBrush(Colors.Green);
                        LastUpdateText.Text =
                            $"Last update: {ModRepository.Instance.Repo.Head.Commits.First().Author.When}";
                        StatusText.Text = "Up to date!";
                        ButtonReset.IsEnabled = false;
                        break;
                }
            });
        }

        private void ButtonRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StatusIcon.Text = SegoeGlyphs.Repeat;
            StatusIcon.Foreground = new SolidColorBrush(Colors.Orange);
            LastUpdateText.Text = "Please wait...";
            StatusText.Text = "Fetching data...";
            ButtonRefresh.IsEnabled = false;
            ButtonReset.IsEnabled = false;
            App.RunInBackgroundThread(ModRepository.Instance.Refresh);
        }

        private void ButtonReset_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.RunInBackgroundThread(ModRepository.Instance.Reset);
        }
    }
}
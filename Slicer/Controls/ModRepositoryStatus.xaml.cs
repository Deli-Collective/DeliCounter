using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Slicer.Backend;

namespace Slicer.Controls
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

        private void Update(ModRepository.State state, Exception e)
        {
            switch (state)
            {
                case ModRepository.State.Error:
                    StatusIcon.Text = SegoeGlyphs.X;
                    StatusIcon.Foreground = new SolidColorBrush(Colors.Red);
                    LastUpdateText.Text = e.Message;
                    StatusText.Text = "Error";
                    break;
                case ModRepository.State.CantUpdate:
                    StatusIcon.Text = "\uF13C";
                    StatusIcon.Foreground = new SolidColorBrush(Colors.Orange);
                    LastUpdateText.Text = e.Message;
                    StatusText.Text = "Offline";
                    break;
                case ModRepository.State.UpToDate:
                    StatusIcon.Text = SegoeGlyphs.Checkmark;
                    StatusIcon.Foreground = new SolidColorBrush(Colors.Green);
                    LastUpdateText.Text =
                        $"Last update: {ModRepository.Instance.Repo.Head.Commits.First().Author.When.ToString()}";
                    StatusText.Text = "Up to date!";
                    break;
            }
        }
    }
}
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using Deli_Counter.Properties;
using LibGit2Sharp;

namespace Deli_Counter.Controls
{
    public partial class ModRepository : UserControl
    {
        public enum RepositoryState
        {
            UpToDate,
            Dirty,
            Offline
        }

        private static readonly string LocalPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "Mods");
        private const string RemotePath = "https://github.com/Deli-Collective/Slicer-Database";

        private Repository? _repo;

        public RepositoryState State { get; private set; }

        public ModRepository()
        {
            InitializeComponent();

            var options = new CloneOptions
            {
                CredentialsProvider = (url, user, cred) => Settings.Default.GitAnonymous ? null : new UsernamePasswordCredentials
                {
                    Username = Settings.Default.GitUsername,
                    Password = Settings.Default.GitPassword
                }
            };

            try
            {
                if (!Directory.Exists(LocalPath))
                    Repository.Clone(RemotePath, LocalPath, options);
                _repo = new Repository(LocalPath);

                UpdateStatus(RepositoryState.UpToDate, "Last Update: " + _repo.Head.Commits.First().Author.When.ToString());
            } catch (LibGit2SharpException e)
            {
                UpdateStatus(RepositoryState.Offline, e.Message);
            }
        }

        private void UpdateStatus(RepositoryState state, string message)
        {
            switch (state)
            {
                case RepositoryState.UpToDate:
                    StatusIcon.Text = "\uF13E";
                    StatusText.Text = "Up to date!";
                    StatusIcon.Foreground = new SolidColorBrush(Colors.Green);
                    break;
                case RepositoryState.Dirty:
                    StatusIcon.Text = "\uF13C";
                    StatusText.Text = "Dirty local";
                    StatusIcon.Foreground = new SolidColorBrush(Colors.Orange);
                    break;
                case RepositoryState.Offline:
                    StatusIcon.Text = "\uF13D";
                    StatusText.Text = "Offline";
                    StatusIcon.Foreground = new SolidColorBrush(Colors.Red);
                    break;

            }

            State = state;
            LastUpdateText.Text = message;
        }
    }
}
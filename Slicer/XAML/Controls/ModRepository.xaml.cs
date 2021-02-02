using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using Deli_Counter.Backend;
using Deli_Counter.Properties;
using LibGit2Sharp;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Deli_Counter.Controls
{
    public partial class ModRepository : UserControl
    {
        public enum RepositoryState
        {
            UpToDate,
            Offline
        }

        private static readonly string LocalPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "Mods");
        private const string RemotePath = "https://github.com/Deli-Collective/Slicer-Database";

        private Repository? _repo;

        public RepositoryState State { get; private set; }

        public ModCategory[]? Categories;

        public event Action? Completed;

        public ModRepository()
        {
            InitializeComponent();


            try
            {
                if (!Directory.Exists(LocalPath))
                    Repository.Clone(RemotePath, LocalPath, new CloneOptions {CredentialsProvider = Settings.Default.GitCredentials});
                _repo = new Repository(LocalPath);

                var signature = new Signature(new Identity(Settings.Default.GitUsername, "unused@email.com"), DateTimeOffset.Now);
                var options = new PullOptions
                {
                    FetchOptions = new FetchOptions {CredentialsProvider = Settings.Default.GitCredentials}
                };

                Commands.Pull(_repo, signature, options);
                
                UpdateStatus(RepositoryState.UpToDate, "Last Update: " + _repo.Head.Commits.First().Author.When);

                ParseMods();
            }
            catch (Exception e)
            {
                UpdateStatus(RepositoryState.Offline, e.Message);
            }

            Completed?.Invoke();
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
                case RepositoryState.Offline:
                    StatusIcon.Text = "\uF13D";
                    StatusText.Text = "Offline";
                    StatusIcon.Foreground = new SolidColorBrush(Colors.Red);
                    break;
            }

            State = state;
            LastUpdateText.Text = message;
        }

        public void ParseMods()
        {
            var deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
            Categories = deserializer.Deserialize<ModCategory[]>(File.ReadAllText(Path.Combine(LocalPath, "index.yaml")));
        }
    }
}
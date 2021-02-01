using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
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

            /*var options = new CloneOptions
            {
                CredentialsProvider = (url, user, cred) => new UsernamePasswordCredentials
                {
                    Username = "ACCESS-TOKEN",
                    Password = string.Empty
                }
            };*/

            //if (!Directory.Exists(LocalPath))
            //    Repository.Clone(RemotePath, LocalPath);
            //_repo = new Repository(LocalPath);
        }
    }
}
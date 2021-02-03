using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static ModCategory[]? Categories;

        public static Mod[]? Mods;

        public ModRepository()
        {
            InitializeComponent();
            Refresh();
        }

        public void Refresh()
        {
            try
            {
                if (!Directory.Exists(LocalPath))
                    Repository.Clone(RemotePath, LocalPath, new CloneOptions { CredentialsProvider = Settings.Default.GitCredentials });
                _repo = new Repository(LocalPath);

                var signature = new Signature(new Identity(Settings.Default.GitUsername, "unused@email.com"), DateTimeOffset.Now);
                var options = new PullOptions
                {
                    FetchOptions = new FetchOptions { CredentialsProvider = Settings.Default.GitCredentials }
                };

                Commands.Pull(_repo, signature, options);

                ParseMods();

                UpdateStatus(RepositoryState.UpToDate, $"Last Update: {_repo.Head.Commits.First().Author.When}\nMods in database: {Mods!.Length}");
            }
            catch (Exception e)
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

            // For each category in the index
            var mods = new List<Mod>();
            foreach (var category in Categories)
            {
                var categoryDir = Path.Combine(LocalPath, category.Path);
                if (!Directory.Exists(categoryDir)) continue;

                // For each mod folder in the category
                foreach (var modDir in Directory.GetDirectories(categoryDir))
                {
                    var mod = new Mod
                    {
                        Guid = Path.GetFileName(modDir),
                        Category = category
                    };


                    // For each version manifest in the directory
                    var list = new List<Mod.ModVersion>();
                    foreach (var manifest in Directory.GetFiles(modDir))
                    {
                        var version = deserializer.Deserialize<Mod.ModVersion>(File.ReadAllText(manifest));
                        version.Mod = mod;
                        list.Add(version);
                    }

                    mod.Versions = list.ToArray();

                    if (mod.Versions.Length > 0) mods.Add(mod);
                }
            }

            Mods = mods.ToArray();
        }
    }
}
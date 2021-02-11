using System;
using System.Windows.Controls;

namespace Slicer.Controls
{
    public partial class ApplicationGitInfo : UserControl
    {
        public ApplicationGitInfo()
        {
            InitializeComponent();
            BuildInfo.Text = Text;
        }

        public static string Text
        {
            get
            {
                var commitDate = DateTime.Parse(ThisAssembly.Git.CommitDate);
                return $"Tag: {ThisAssembly.Git.Tag}\n" +
                       $"Branch: {ThisAssembly.Git.Branch}{(ThisAssembly.Git.IsDirty ? "-dirty" : "")} ({ThisAssembly.Git.Commit})\n" +
                       $"Commit Date: {commitDate}";
            }
        }
    }
}
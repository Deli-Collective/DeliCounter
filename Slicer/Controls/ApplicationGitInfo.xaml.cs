using System;
using System.Windows.Controls;

namespace Slicer.Controls
{
    public partial class ApplicationGitInfo : UserControl
    {
        public ApplicationGitInfo()
        {
            InitializeComponent();

            var commitDate = DateTime.Parse(ThisAssembly.Git.CommitDate);
            BuildInfo.Text = $"Tag: {ThisAssembly.Git.Tag}\n" +
                             $"Branch: {ThisAssembly.Git.Branch}{(ThisAssembly.Git.IsDirty ? "-dirty" : "")} ({ThisAssembly.Git.Commit})\n" +
                             $"Commit Date: {commitDate}";
        }
    }
}
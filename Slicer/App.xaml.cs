using System;
using Slicer.Backend;

namespace Slicer
{
    public partial class App
    {
        protected override void OnActivated(EventArgs e)
        {
            var refresh = ModRepository.Instance.Refresh();
        }
    }
}
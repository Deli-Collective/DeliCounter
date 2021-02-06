namespace Slicer.Controls
{
    public partial class AlertDialogue
    {
        public AlertDialogue(string title, string message)
        {
            InitializeComponent();
            Message.Text = message;
            Root.Title = title;
        }
    }
}
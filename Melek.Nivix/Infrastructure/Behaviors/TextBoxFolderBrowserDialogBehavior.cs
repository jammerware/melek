using System.Windows.Forms;
using System.Windows.Interactivity;

namespace Nivix.Infrastructure.Behaviors
{
    public class TextBoxFolderBrowserDialogBehavior : Behavior<System.Windows.Controls.TextBox>
    {
        private bool _DialogIsOpen = false;

        protected override void OnAttached()
        {
            AssociatedObject.GotFocus += (thisTextBox, isFocusedAsShit) => {
                if (!_DialogIsOpen) {
                    _DialogIsOpen = true;

                    FolderBrowserDialog dlg = new FolderBrowserDialog();

                    // Show open file dialog box
                    DialogResult result = dlg.ShowDialog();

                    if (result == DialogResult.OK) { AssociatedObject.Text = dlg.SelectedPath; }
                    _DialogIsOpen = false;
                }
            };
        }
    }
}
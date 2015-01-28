using System.Windows.Controls;
using System.Windows.Interactivity;
using Microsoft.Win32;

namespace Nivix.Infrastructure.Behaviors
{
    public class TextBoxFileDialogBehavior : Behavior<TextBox>
    {
        private bool _DialogIsOpen = false;

        public string TargetFileDescription { get; set; }
        public string TargetFileExtension { get; set; }

        protected override void OnAttached()
        {
            AssociatedObject.GotFocus += (thisTextBox, isFocusedAsShit) => {
                if (!_DialogIsOpen) {
                    _DialogIsOpen = true;

                    OpenFileDialog dlg = new OpenFileDialog();
                    if (TargetFileDescription != string.Empty && TargetFileExtension != string.Empty) {
                        dlg.Filter = TargetFileDescription + "|*." + TargetFileExtension;
                    }

                    // Show open file dialog box
                    bool? result = dlg.ShowDialog();

                    if (result == true) { AssociatedObject.Text = dlg.FileName; }
                    _DialogIsOpen = false;
                }
            };
        }
    }
}
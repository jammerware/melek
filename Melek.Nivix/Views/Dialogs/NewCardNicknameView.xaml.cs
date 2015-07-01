using FirstFloor.ModernUI.Windows.Controls;
using System.Windows.Controls;

namespace Nivix.Views.Dialogs
{
    public partial class NewCardNicknameView : ModernDialog
    {
        public NewCardNicknameView()
        {
            InitializeComponent();

            // define the dialog buttons
            this.Buttons = new Button[] { this.OkButton, this.CancelButton };
        }
    }
}
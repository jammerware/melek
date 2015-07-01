using FirstFloor.ModernUI.Windows.Controls;
using System.Windows.Controls;

namespace Nivix.Views.Dialogs
{
    public partial class NewSetView : ModernDialog
    {
        public NewSetView()
        {
            InitializeComponent();

            this.Buttons = new Button[] { this.OkButton, this.CancelButton };
        }
    }
}

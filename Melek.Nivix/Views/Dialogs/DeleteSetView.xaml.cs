using FirstFloor.ModernUI.Windows.Controls;
using System.Windows.Controls;

namespace Nivix.Views.Dialogs
{
    public partial class DeleteSetView : ModernDialog
    {
        public DeleteSetView()
        {
            InitializeComponent();

            this.Buttons = new Button[] { this.YesButton, this.NoButton };
        }
    }
}

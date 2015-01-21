using System.Windows;
using System.Windows.Media;
using FirstFloor.ModernUI.Presentation;

namespace Nivix
{
    public partial class App : Application
    {
        private void this_Startup(object sender, StartupEventArgs e)
        {
            // set up MUI theme
            Color myAccentColor = (Color)App.Current.FindResource("MyAccentColor");
            AppearanceManager.Current.AccentColor = myAccentColor;
        }
    }
}
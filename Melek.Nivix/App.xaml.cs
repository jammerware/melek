using FirstFloor.ModernUI.Presentation;
using System.Windows;
using System.Windows.Media;

namespace Melek.Nivix
{
    public partial class App : Application
    {
        private void this_Startup(object sender, StartupEventArgs e)
        {
            // set up MUI theme
            AppearanceManager.Current.AccentColor = (Color)App.Current.FindResource("MyAccentColor");
        }
    }
}
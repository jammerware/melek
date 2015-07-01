using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Nivix.Infrastructure.Behaviors
{
    public class ToggleVisibilityOnClickBehavior : Behavior<Button>
    {
        public FrameworkElement TargetElement
        {
            get { return (FrameworkElement)GetValue(TargetElementProperty); }
            set { SetValue(TargetElementProperty, value); }
        }

        public static readonly DependencyProperty TargetElementProperty = DependencyProperty.Register(
            "TargetElement",
            typeof(FrameworkElement),
            typeof(ToggleVisibilityOnClickBehavior),
            new PropertyMetadata(null)
        );

        protected override void OnAttached()
        {
            AssociatedObject.Click += (someoneClicked, someShit) => {
                if (TargetElement != null) {
                    TargetElement.Visibility = (TargetElement.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
                }
            };
        }
    }
}
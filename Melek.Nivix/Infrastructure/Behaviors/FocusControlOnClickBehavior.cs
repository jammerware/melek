using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Nivix.Infrastructure.Behaviors
{
    public class FocusControlOnClickBehavior : Behavior<Button>
    {
        public FrameworkElement FocusTarget
        {
            get { return (FrameworkElement)GetValue(FocusTargetProperty); }
            set { SetValue(FocusTargetProperty, value); }
        }

        public static readonly DependencyProperty FocusTargetProperty = DependencyProperty.Register(
            "FocusTarget", 
            typeof(FrameworkElement), 
            typeof(FocusControlOnClickBehavior), 
            new PropertyMetadata(null)
        );

        protected override void OnAttached()
        {
            AssociatedObject.Click += (someoneFucking, clickedTheButton) => {
                if (FocusTarget != null) FocusTarget.Focus();
            };
        }
    }
}
using Microsoft.Xaml.Behaviors; // Requires Microsoft.Xaml.Behaviors.Wpf NuGet
using System.Windows.Controls;
using System.Windows.Input;

namespace ExplorerTags
{
    public class TripleClickSelectAllBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 3)
            {
                AssociatedObject.SelectAll();
                e.Handled = true; // Prevents the click from de-selecting text immediately
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
            base.OnDetaching();
        }
    }
}

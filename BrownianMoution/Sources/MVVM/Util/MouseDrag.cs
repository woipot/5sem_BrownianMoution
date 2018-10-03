using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BrownianMoution.Sources.MVVM.Util
{
    public class MouseDrag
    {
        public static DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command",
                typeof(ICommand),
                typeof(MouseDrag),
                new UIPropertyMetadata(CommandChanged));

        public static DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter",
                typeof(object),
                typeof(MouseDrag),
                new UIPropertyMetadata(null));

        public static void SetCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        public static void SetCommandParameter(DependencyObject target, object value)
        {
            target.SetValue(CommandParameterProperty, value);
        }
        public static object GetCommandParameter(DependencyObject target)
        {
            return target.GetValue(CommandParameterProperty);
        }

        private static void CommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (target is Thumb control)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    control.DragDelta += OnMouseDrag;
                    //control.MouseDoubleClick += OnMouseDrag;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    control.DragDelta -= OnMouseDrag;
                }
            }
        }

        private static void OnMouseDrag(object sender, RoutedEventArgs e)
        {
            Control control = sender as Thumb;
            var command = (ICommand)control?.GetValue(CommandProperty);
            var parameter = new MouseDragArgs() { EArgs = e, Sender = sender };

            object commandParameter = parameter;
            command?.Execute(commandParameter);
        }
    }
}

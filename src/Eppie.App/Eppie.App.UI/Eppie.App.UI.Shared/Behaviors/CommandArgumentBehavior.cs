using Microsoft.Xaml.Interactivity;
using System.Windows.Input;

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#endif

namespace Eppie.App.UI.Behaviors
{
    public class CommandArgumentBehavior : Behavior<Button>
    {
        public static readonly DependencyProperty CommandProperty =
                    DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(CommandArgumentBehavior), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandArgumentProperty =
            DependencyProperty.Register(nameof(CommandArgument), typeof(ICommandArgument), typeof(CommandArgumentBehavior), new PropertyMetadata(null));

        public ICommandArgument CommandArgument
        {
            get { return (ICommandArgument)GetValue(CommandArgumentProperty); }
            set { SetValue(CommandArgumentProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.Click += OnClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= OnClick;
        }

        protected virtual void OnClick(object sender, RoutedEventArgs e)
        {
            if (CommandArgument?.TryGetValue(sender, e, out object value) == true && Command?.CanExecute(value) == true)
            {
                Command?.Execute(value);
            }
        }
    }

    public interface ICommandArgument
    {
        bool TryGetValue(object sender, RoutedEventArgs e, out object value);
    }

    public abstract class CommandArgument<T> : ICommandArgument
    {
        public T Value { get; set; }

        public virtual bool TryGetValue(object sender, RoutedEventArgs e, out object value)
        {
            value = Value;
            return true;
        }
    }
}

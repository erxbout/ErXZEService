using System.Windows.Input;
using Xamarin.Forms;

namespace ErXZEService.Controls
{
    [ContentProperty(nameof(ContentView))]
    public class ClickableStackLayout : StackLayout
    {
        private readonly TapGestureRecognizer _defaultTapGesture;

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ClickableStackLayout), default(ICommand));

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ClickableStackLayout), default(object));

        public static readonly BindableProperty ContentViewProperty = BindableProperty.Create(nameof(ContentView), typeof(View), typeof(ClickableStackLayout), null, propertyChanged: (bindable, oldValue, newValue) =>
        {
            (bindable as ClickableStackLayout).SetContentView(oldValue as View);
            (bindable as ClickableStackLayout).OnTouchHandlerViewChanged();
        });

        public View ContentView
        {
            get => GetValue(ContentViewProperty) as View;
            set => SetValue(ContentViewProperty, value);
        }

        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }

        public ClickableStackLayout()
        {
            _defaultTapGesture = new TapGestureRecognizer
            {
                CommandParameter = this,
                Command = new Command(p =>
                {
                    Command?.Execute(CommandParameter);
                })
            };

            OnTouchHandlerViewChanged();
        }

        private void SetContentView(View oldView)
        {
            if (oldView != null)
            {
                Children.Remove(oldView);
            }
            Children.Insert(0, ContentView);
        }

        private void OnTouchHandlerViewChanged()
        {
            var gesturesList = ContentView?.GestureRecognizers;
            gesturesList?.Remove(_defaultTapGesture);
            ContentView?.GestureRecognizers.Remove(_defaultTapGesture);
            gesturesList?.Add(_defaultTapGesture);
        }
    }
}

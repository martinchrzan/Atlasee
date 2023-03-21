using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using VisFileManager.Messenger;

namespace VisFileManager.Behaviors
{
    public class ItemsStyleChangerBehavior : Behavior<ItemsControl>
    {
        private static bool _gridViewActive = true;

        private static Style _gridStyle;
        private static Style _listStyle;
        private static Style _myComputerStyle;

        public bool MyComputerView
        {
            get { return (bool)GetValue(MyComputerViewProperty); }
            set
            {
                SetValue(MyComputerViewProperty, value);
            }
        }

        public static readonly DependencyProperty MyComputerViewProperty = DependencyProperty.Register(
          "MyComputerView", typeof(bool), typeof(ItemsStyleChangerBehavior), new PropertyMetadata(MyComputerViewChanged));

        private static void MyComputerViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var caller = (ItemsStyleChangerBehavior)d;

            if (_gridViewActive)
            {
                if ((bool)e.NewValue)
                {
                    caller.AssociatedObject.Style = _myComputerStyle;
                }
                else if (!(bool)e.NewValue)
                {
                    caller.AssociatedObject.Style = _gridStyle;
                }
            }
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            var messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
            messenger.Subscribe<bool>(MessageIds.GridViewActive, (gridViewActive) =>
            {
                _gridViewActive = gridViewActive;

                if (gridViewActive)
                {
                    AssociatedObject.Style = MyComputerView ? _myComputerStyle : _gridStyle;
                }
                else
                {
                    AssociatedObject.Style = _listStyle;
                }
            });

            _gridStyle = ((FrameworkElement)AssociatedObject).FindResource("GridViewStyleGenericTemplate") as Style;
            _listStyle = ((FrameworkElement)AssociatedObject).FindResource("ListViewStyleTemplate") as Style;
            _myComputerStyle = ((FrameworkElement)AssociatedObject).FindResource("GridViewStyleMyComputerTemplate") as Style;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            base.OnDetaching();
        }
    }
}

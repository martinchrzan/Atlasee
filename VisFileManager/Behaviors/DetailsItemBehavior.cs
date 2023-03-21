using Microsoft.Xaml.Behaviors;
using System.Windows;
using VisFileManager.Controls;
using VisFileManager.Messenger;
using VisFileManager.Messenger.Messages;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.Behaviors
{
    public class DetailsItemBehavior : Behavior<FrameworkElement>
    {
        private IMessenger _messenger;

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
            (AssociatedObject as GazeActivableBorder).GazeChanged -= DetailsItemBehavior_GazeChanged;

        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            (AssociatedObject as GazeActivableBorder).GazeChanged += DetailsItemBehavior_GazeChanged;
            _messenger = Bootstraper.Container.GetExportedValue<IMessenger>();
        }

        private void DetailsItemBehavior_GazeChanged(object sender, System.EventArgs e)
        {
            if ((AssociatedObject as GazeActivableBorder).HasGaze)
            {
                SendMessage();
            }
        }

        private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            Point absolutePosition = AssociatedObject.TransformToAncestor(Application.Current.MainWindow)
                              .Transform(new Point(0, 0));

            var message = DetailsItemHoveredMessage.Create(AssociatedObject.DataContext as IDetailsItemViewModel, absolutePosition);

            _messenger.Send(MessageIds.DetailsItemHovered, message);
        }
    }
}

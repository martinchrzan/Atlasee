using System.Windows.Input;
using System.Windows.Media.Imaging;
using VisFileManager.Common;
using VisFileManager.Messenger;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.ViewModels.DetailsPanel
{
    public class SimpleDetailsItemViewModel : ISimpleDetailsItemViewModel
    {
        private readonly IMessenger _messenger;

        public SimpleDetailsItemViewModel(ICommand invokeCommand, BitmapSource icon, string name, int level, IMessenger messenger, bool closeDetailsOnCommand)
        {
            InvokeCommand = new RelayCommand(invokeCommand.CanExecute, (o) =>
            {
                invokeCommand.Execute(o);
                if (closeDetailsOnCommand)
                {
                    messenger.Send(MessageIds.CloseDetailsRequestMessage, true);
                }
            }); 

            Icon = icon;
            Name = name;
            Level = level;
            _messenger = messenger;
        }

        public string Name { get; }

        public BitmapSource Icon { get; }

        public ICommand InvokeCommand { get; }

        public int Level { get; }
    }
}

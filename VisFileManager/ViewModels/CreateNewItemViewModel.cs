using System.ComponentModel.Composition;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.FileSystemHelpers;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModels
{
    [Export(typeof(ICreateNewItemViewModel))]
    public class CreateNewItemViewModel : ICreateNewItemViewModel
    {
        private readonly INewItemsCreator _newItemsCreator;
        private readonly IGlobalFileManager _globalFileManager;

        [ImportingConstructor]
        public CreateNewItemViewModel(INewItemsCreator newItemsCreator, IGlobalFileManager globalFileManager)
        {
            CreateNewFolderCommand = new RelayCommand(CreateNewFolder);
            CreateNewTextFileCommand = new RelayCommand(CreateNewTextFile);

            _newItemsCreator = newItemsCreator;
            _globalFileManager = globalFileManager;
        }

        public ICommand CreateNewFolderCommand { get; }

        public ICommand CreateNewTextFileCommand { get; }

        private async void CreateNewTextFile()
        {
            await _newItemsCreator.CreateNewTextFile(_globalFileManager.CurrentPath);
        }

        private async void CreateNewFolder()
        {
            await _newItemsCreator.CreateNewFolder(_globalFileManager.CurrentPath);
        }
    }
}

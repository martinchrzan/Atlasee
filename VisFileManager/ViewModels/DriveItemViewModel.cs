using System.IO;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.Extensions;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;

namespace VisFileManager.ViewModels
{
    public class DriveItemViewModel : IDriveItemViewModel
    {
        private readonly DriveInfo _driveInfo;
        private readonly IInvokeHelper _invokeHelper;
        private readonly IMessenger _messenger;
        private FormattedPath _formattedPath;

        public DriveItemViewModel(DriveInfo driveInfo, IInvokeHelper invokeHelper, IMessenger messenger)
        {
            _driveInfo = driveInfo;
            _invokeHelper = invokeHelper;
            _messenger = messenger;
            CachedFullPath = _driveInfo.RootDirectory.FullName;
        }

        public string EmptySpaceInKB => FileInfoHelper.ToSizeWithSuffixWithoutOriginalValue((ulong)_driveInfo.AvailableFreeSpace);

        public string SizeInKB => FileInfoHelper.ToSizeWithSuffixWithoutOriginalValue((ulong)_driveInfo.TotalSize);

        public string Name => FullFormattedPath.Name;

        public FormattedPath FullFormattedPath
        {
            get
            {
                if (_formattedPath == null)
                {
                    _formattedPath = FormattedPath.CreateFormattedPath(_driveInfo);
                }
                return _formattedPath;
            }
        }

        public string Type => _driveInfo.GetDriveType();

        public string DriveFormat => _driveInfo.DriveFormat;

        public ICommand OpenCommand => new RelayCommand(OpenFileItem);

        public ICommand OpenPropertiesCommand => new RelayCommand(OpenProperties);

        public double PercentageOccupied => ((_driveInfo.TotalSize - _driveInfo.AvailableFreeSpace) / (double)_driveInfo.TotalSize) * 100;

        public string CachedFullPath { get; }

        private void OpenFileItem()
        {
            _invokeHelper.Invoke(FullFormattedPath);
        }

        private void OpenProperties() => _messenger.Send(MessageIds.OpenDriveDetailsMessage, _driveInfo);

    }
}

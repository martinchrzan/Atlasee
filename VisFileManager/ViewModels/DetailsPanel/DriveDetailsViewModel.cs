using System.IO;
using VisFileManager.Extensions;
using VisFileManager.Helpers;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.ViewModels.DetailsPanel
{
    public class DriveDetailsViewModel : IDriveDetailsViewModel
    {
        private readonly DriveInfo _driveInfo;

        public DriveDetailsViewModel(DriveInfo driveInfo)
        {
            _driveInfo = driveInfo;
            FullPath = FormattedPath.CreateFormattedPath(_driveInfo);
        }

        public string DriveType => _driveInfo.GetDriveType();

        public string DriveFormat => _driveInfo.DriveFormat;

        public string AvailableFreeSpace => FileInfoHelper.ToSizeWithSuffixWithoutOriginalValue((ulong)_driveInfo.AvailableFreeSpace);

        public string TotalSize => FileInfoHelper.ToSizeWithSuffixWithoutOriginalValue((ulong)_driveInfo.TotalSize);

        public string Name
        {
            get
            {
                return _driveInfo.GetDriveName();
            }
            set
            {//no support!
            }
        }

        public double PercentageOccupied => (_driveInfo.TotalSize - _driveInfo.AvailableFreeSpace) / (double)_driveInfo.TotalSize;

        public string UsedSpace => FileInfoHelper.ToSizeWithSuffixWithoutOriginalValue((ulong)(_driveInfo.TotalSize - _driveInfo.AvailableFreeSpace));

        public FormattedPath FullPath { get; set; }
    }
}

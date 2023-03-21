namespace VisFileManager.ViewModelContracts.DetailsPanel
{
    public interface IDriveDetailsViewModel : IDetailsViewModelBase
    {
        string DriveType { get; }

        string DriveFormat { get; }

        string AvailableFreeSpace { get; }

        string UsedSpace { get; }

        string TotalSize { get; }

        double PercentageOccupied { get; }
    }
}

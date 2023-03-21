namespace VisFileManager.ViewModelContracts
{
    public interface IDriveItemViewModel : IFileAndDriveItemViewModelBase
    {
        string EmptySpaceInKB { get; }

        double PercentageOccupied { get; }

        string DriveFormat { get; }
    }
}

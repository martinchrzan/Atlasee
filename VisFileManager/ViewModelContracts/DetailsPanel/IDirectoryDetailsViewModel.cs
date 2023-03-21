namespace VisFileManager.ViewModelContracts.DetailsPanel
{
    public interface IDirectoryDetailsViewModel : IDetailsViewModelBase
    {
        string Created { get; }

        string DirectorySize { get; }

        string Contains { get; }
    }
}

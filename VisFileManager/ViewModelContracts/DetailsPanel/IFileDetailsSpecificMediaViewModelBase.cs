namespace VisFileManager.ViewModelContracts.DetailsPanel
{
    public interface IFileDetailsSpecificMediaViewModel : ISpecificFileDetailsViewModel
    {
        string Duration { get; }

        string Bitrate { get; }
    }
}

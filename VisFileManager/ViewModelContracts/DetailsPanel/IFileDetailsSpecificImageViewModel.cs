namespace VisFileManager.ViewModelContracts.DetailsPanel
{
    public interface IFileDetailsSpecificImageViewModel : ISpecificFileDetailsViewModel
    {
        int ImageWidth { get; }

        int ImageHeight { get; }

        uint? ImageBitness { get; }
    }
}

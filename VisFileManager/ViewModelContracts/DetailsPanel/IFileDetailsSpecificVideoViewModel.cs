namespace VisFileManager.ViewModelContracts.DetailsPanel
{
    public interface IFileDetailsSpecificVideoViewModel : IFileDetailsSpecificMediaViewModel
    {
        uint? VideoWidth { get; }

        uint? VideoHeigh { get;}

        string FrameRate { get; }
    }
}

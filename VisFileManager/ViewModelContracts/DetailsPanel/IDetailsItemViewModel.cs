namespace VisFileManager.ViewModelContracts.DetailsPanel
{
    public interface IDetailsItemViewModel
    {
        string Name { get; }

        // inidication where is this item positioned, 0 means root, 1 is its child etc.
        int Level { get; }
    }
}

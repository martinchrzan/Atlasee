using System;
using VisFileManager.Validators;

namespace VisFileManager.ViewModelContracts.DetailsPanel
{
    public interface IDetailsViewModelBase
    {
        string Name { get; set; }

        FormattedPath FullPath { get; set; }
    }
}

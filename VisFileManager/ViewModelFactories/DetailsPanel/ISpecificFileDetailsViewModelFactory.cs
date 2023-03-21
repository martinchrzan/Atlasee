using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.ViewModelFactories.DetailsPanel
{
    public interface ISpecificFileDetailsViewModelFactory
    {
        ISpecificFileDetailsViewModel CreateSpecifiFileDetailsViewModel(FormattedPath path);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisFileManager.ViewModelContracts.DetailsPanel
{
    public interface IFileDetailsViewModel : IDetailsViewModelBase
    {
        string FileSize { get; }

        string FileSizeOnDisk { get; }

        string TypeOfFile { get; }
        
        string Modified { get; }

        string Accessed { get; }

        string Created { get; }

        ISpecificFileDetailsViewModel SpecificFileDetailsViewModel { get; }
    }
}

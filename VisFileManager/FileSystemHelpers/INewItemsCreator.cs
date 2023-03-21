using System.Threading.Tasks;
using VisFileManager.Validators;

namespace VisFileManager.FileSystemHelpers
{
    public interface INewItemsCreator
    {
        Task<string> CreateNewFolder(FormattedPath rootDirectory);

        Task<string> CreateNewTextFile(FormattedPath rootDirectory);
    }
}

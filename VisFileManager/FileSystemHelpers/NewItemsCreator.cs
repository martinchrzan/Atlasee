using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using VisFileManager.Shared;
using VisFileManager.Validators;

namespace VisFileManager.FileSystemHelpers
{
    [Export(typeof(INewItemsCreator))]
    public class NewItemsCreator : INewItemsCreator
    {
        private const string NewFileName = "New text file.txt";
        private const string NewFolderName = "New folder";
        private readonly IFileOperationsManager _fileOperationsManager;

        [ImportingConstructor]
        public NewItemsCreator(IFileOperationsManager fileOperationsManager)
        {
            _fileOperationsManager = fileOperationsManager;
        }

        public async Task<string> CreateNewFolder(FormattedPath rootDirectory)
        {
            return await Task.Run(async () =>
            {
                var directoryName = Path.Combine(rootDirectory.Path, NewFolderName);
                var directoryNameAvailable = UniqueNameGeneratorHelper.NextAvailableName(directoryName);

                var result = await _fileOperationsManager.CreateNewFolder(directoryNameAvailable, new System.Threading.CancellationTokenSource());

                return result ? directoryNameAvailable : string.Empty; 
            });
        }

        public async Task<string> CreateNewTextFile(FormattedPath rootDirectory)
        {
            return await Task.Run(async () =>
            {
                var fullFileName = Path.Combine(rootDirectory.Path, NewFileName);
                var fileNameAvailable = UniqueNameGeneratorHelper.NextAvailableName(fullFileName);

                var result = await _fileOperationsManager.CreateNewTextFile(fileNameAvailable, new System.Threading.CancellationTokenSource());

                return result ? fileNameAvailable : string.Empty;
            });
        }
    }
}

using System;
using System.IO;
using System.Linq;
using VisFileManager.Common;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.ViewModels.DetailsPanel
{
    public class DetailsViewModelBase : ViewModelBase, IDetailsViewModelBase
    {
        private readonly IFileOperationsManager _fileOperationsManager;
        private string _name;

        public DetailsViewModelBase(FormattedPath path, IFileOperationsManager fileOperationsManager)
        {
            FullPath = path;
            _fileOperationsManager = fileOperationsManager;
            _name = FullPath.Name;
        }

        public FormattedPath FullPath { get; set; }

        public string Name { get
            {
                return _name;
            }
            set
            {
                if(FileNameValidator.ValidateName(value, FullPath))
                {
                    var requestedName = Path.Combine(Path.GetDirectoryName(FullPath.Path), value);

                    if(requestedName != FullPath.Path)
                    {
                        var cancellationToken = new System.Threading.CancellationTokenSource();
                        _fileOperationsManager.Rename(FullPath, requestedName, cancellationToken, (result) =>
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                if (result)
                                {
                                    _name = value;
                                    FullPath = FormattedPath.CreateFormattedPath(requestedName);
                                    OnPropertyChanged(nameof(FullPath));
                                    OnPropertyChanged();
                                }
                            });
                            cancellationToken.Dispose();
                        });

                        
                    }
                }
            }
        }
    }
}

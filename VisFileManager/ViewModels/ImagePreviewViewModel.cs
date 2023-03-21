using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VisFileManager.Common;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Helpers;
using VisFileManager.Messenger;
using VisFileManager.Settings;
using VisFileManager.Validators;
using VisFileManager.ViewModelContracts;
using VisFileManager.ViewModelFactories;

namespace VisFileManager.ViewModels
{
    [Export(typeof(IImagePreviewViewModel))]
    public class ImagePreviewViewModel : ViewModelBase, IImagePreviewViewModel
    {
        private const int MsPerSecond = 1000;
        private IThrottledActionInvoker _selectedImageThrottledActionInvoker;
        private IThrottledActionInvoker _selectedImageLowResThrottledActionInvoker;
        private IThrottledActionInvoker _slideshowScheduledActionInvoker;
        private readonly IThrottledActionInvokerFactory _throttledActionInvokerFactory;
        private readonly IGlobalFileManager _globalFileManager;
        private readonly FileInfoHelper _fileInfoHelper;
        private readonly IMessenger _messenger;
        private readonly IImagePreviewItemViewModelFactory _imagePreviewItemViewModelFactory;
        private readonly IUserSettings _userSettings;
        private IImagePreviewItemViewModel _selectedImageInList;
        private bool _nullSet;
        private int _slideshowDelayInSeconds;
        private bool _inSlideshowMode = false;


        [ImportingConstructor]
        public ImagePreviewViewModel(IThrottledActionInvokerFactory throttledActionInvokerFactory, 
            IGlobalFileManager globalFileManager, 
            FileInfoHelper fileInfoHelper, 
            IMessenger messenger, 
            IImagePreviewItemViewModelFactory imagePreviewItemViewModelFactory,
            IUserSettings userSettings)
        {
            _throttledActionInvokerFactory = throttledActionInvokerFactory;
            _globalFileManager = globalFileManager;
            _fileInfoHelper = fileInfoHelper;
            _messenger = messenger;
            _imagePreviewItemViewModelFactory = imagePreviewItemViewModelFactory;
            _userSettings = userSettings;

            Images = new RangeObservableCollection<IImagePreviewItemViewModel>();

            _closeSubscription = messenger.Subscribe<bool>(MessageIds.GalleryCloseWindow, (b) =>
            {
                _selectedImageLowResThrottledActionInvoker.Dispose();
                _selectedImageThrottledActionInvoker.Dispose();
            });

            CloseCommand = new RelayCommand(() => { messenger.Send<bool>(MessageIds.GalleryCloseWindow, true);
                _selectedImageThrottledActionInvoker.Dispose();
                _selectedImageLowResThrottledActionInvoker.Dispose();
                _slideshowScheduledActionInvoker.Dispose();
            });

            RotateCommand = new RelayCommand(() => { messenger.Send<bool>(MessageIds.GalleryRotateImage, true); });
            SlideshowCommand = new RelayCommand(() => {
                _inSlideshowMode = true;
                SetSelectedImage(SelectedImageInList);
                messenger.Send<bool>(MessageIds.GallerySlideshowMode, true);
            });
        }

        public void Initialize()
        {
            _inSlideshowMode = false;
            _selectedImageThrottledActionInvoker = _throttledActionInvokerFactory.CreateThrottledActionInvoker();
            _selectedImageLowResThrottledActionInvoker = _throttledActionInvokerFactory.CreateThrottledActionInvoker();
            _slideshowScheduledActionInvoker = _throttledActionInvokerFactory.CreateThrottledActionInvoker();
            _slideshowDelayInSeconds = _userSettings.SlideshowDuration.Value;

            LoadImageFiles(_globalFileManager, _fileInfoHelper);
        }

        public RangeObservableCollection<IImagePreviewItemViewModel> Images { get; private set; }

        private Guid _closeSubscription;

        public IImagePreviewItemViewModel SelectedImageInList
        {
            get
            {
                return _selectedImageInList;
            }
            set
            {
                _selectedImageInList = value;
                OnPropertyChanged();

                if (!_nullSet)
                {
                    SelectedImage = null;
                    SelectedImageLowRes = null;
                    OnPropertyChanged("SelectedImage");
                    OnPropertyChanged("SelectedImageLowRes");
                    _nullSet = true;
                }
                _selectedImageThrottledActionInvoker.ScheduleAction(() => {
                    SelectedImage = _selectedImageInList;
                    OnPropertyChanged("SelectedImage");
                    SelectedImageLowRes = null;
                    OnPropertyChanged("SelectedImageLowRes");
                }, 200);

                _selectedImageLowResThrottledActionInvoker.ScheduleAction(() =>
                {
                    SelectedImageLowRes = _selectedImageInList;
                    OnPropertyChanged("SelectedImageLowRes");
                    _messenger.Send<bool>(MessageIds.GalleryPrepareForNewImage, true);
                    _nullSet = false;
                }, 50);

                SetSelectedImage(value);
            }
        }

        public IImagePreviewItemViewModel SelectedImage { get; set; }

        public IImagePreviewItemViewModel SelectedImageLowRes { get; set; }

        public ICommand SlideshowCommand { get; set; }

        public ICommand CloseCommand { get; }

        public ICommand RotateCommand { get; }

        public override void Unsubscribe()
        {
            _messenger.Unsubscribe(_closeSubscription);
            base.Unsubscribe();
        }

        private void SetSelectedImage(IImagePreviewItemViewModel image)
        {
            if (_inSlideshowMode)
            {
                SelectedImage = image;
                _selectedImageInList = image;

                OnPropertyChanged("SelectedImage");
                OnPropertyChanged("SelectedImageInList");

                if (Images.IndexOf(image) < Images.Count-1)
                {
                    _slideshowScheduledActionInvoker.ScheduleAction(() =>
                    {
                        var currentIndex = Images.IndexOf(SelectedImage);
                        if (Images.Count > currentIndex + 1)
                        {
                            SetSelectedImage(Images[currentIndex + 1]);
                        }
                    }, _slideshowDelayInSeconds * MsPerSecond);
                }
            }
        }

        private void LoadImageFiles(IGlobalFileManager globalFileManager, FileInfoHelper fileInfoHelper)
        {
            Images.Clear();
            var (directories, files) = Task.Run(() => globalFileManager.GetAllFileEntries(globalFileManager.CurrentPath)).Result;
            var images = new List<IImagePreviewItemViewModel>();
            foreach (var file in files)
            {
                if (fileInfoHelper.GetGeneralFileType(FormattedPath.CreateFormattedPath(file.FullName)) == FileInfoHelper.PerceivedTypeEnum.Image)
                {
                    images.Add(_imagePreviewItemViewModelFactory.CreateImagePreviewItemViewModel(file.FullName, (SelectedImage) =>
                    {
                        SelectedImageInList = SelectedImage;
                    }));
                }
            }
            Images.AddRange(images);

            if (Images.Count > 0)
            {
                SelectedImage = Images.First();
                _selectedImageInList = SelectedImage;
                OnPropertyChanged("SelectedImageInList");
                OnPropertyChanged("SelectedImage");
            }
            else
            {
                SelectedImage = null;
            }
            OnPropertyChanged("SelectedImage");
        }
    }
}

using System.Windows;
using System.Windows.Controls;

namespace VisFileManager.Behaviors
{
    public static class GalleryCacheProvider
    {
        private static ItemsControl _galleryItemsControl;
        private static ScrollViewer _scrollViewer;

        public static ItemsControl GetGalleryItemsControl(FrameworkElement childItem)
        {
            if (_galleryItemsControl == null)
            {
                _galleryItemsControl = ItemsControlParentProvider.FindParent<ItemsControl>(childItem);
            }
            return _galleryItemsControl;
        }

        public static ScrollViewer GetGalleryScrollViewer(FrameworkElement childItem)
        {
            if(_scrollViewer == null)
            {
                _scrollViewer = ItemsControlParentProvider.FindParent<ScrollViewer>(childItem);
            }
            return _scrollViewer;
        }

        public static void ClearCache()
        {
            _galleryItemsControl = null;
            _scrollViewer = null;
        }
    }
}

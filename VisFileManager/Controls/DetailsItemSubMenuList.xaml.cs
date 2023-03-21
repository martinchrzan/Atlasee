using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using VisFileManager.Common;
using VisFileManager.Helpers;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.Controls
{
    /// <summary>
    /// Interaction logic for DetailsItemSubMenuList.xaml
    /// </summary>
    public partial class DetailsItemSubMenuList : Border, INotifyPropertyChanged
    {
        private object _openLock = new object();
        private string _title;

        public event PropertyChangedEventHandler PropertyChanged;

        public DetailsItemSubMenuList()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void OpenMenu(Point parentMenuPosition)
        {
            lock (_openLock)
            {
                if (!Opened)
                {
                    SetMargin(parentMenuPosition);
                    var sb = (Storyboard)FindResource("openMenu");
                    sb.Begin();
                    Opened = true;
                }
            }
        }

        public void CloseMenu()
        {
            lock (_openLock)
            {
                if (Opened)
                {
                    var sb = (Storyboard)FindResource("closeMenu");
                    sb.Begin();
                    ChildItems.Clear();
                    Opened = false;
                }
            }
        }

        public bool Opened { get; private set; }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public List<IDetailsItemViewModel> ChildItems { get; } = new List<IDetailsItemViewModel>();

        public void SetItems(IEnumerable<IDetailsItemViewModel> childItems, string name)
        {
            foreach(var item in childItems)
            {
                ChildItems.Add(item);
            }
            Title = name;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //set margin so sub menu is vertically aligned with parent, checks window size as well so it does not overflow below visible part
        private void SetMargin(Point parentAbsolutePosition)
        {
            var applicationHeight = ApplicationInfoProvider.GetApplicationContentHeight();

            // counting number of items by using hardcoded sizes
            var guessSize = ChildItems.Count * 52;
            // add top border
            guessSize += 30;

            if(parentAbsolutePosition.Y-65 + guessSize > applicationHeight)
            {
                Margin = new Thickness(0, applicationHeight - guessSize, 0, 0);
            }
            else
            {
                Margin = new Thickness(0, parentAbsolutePosition.Y-65, 0, 0);
            }
        }
    }
}

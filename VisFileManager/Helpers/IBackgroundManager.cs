using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisFileManager.Helpers
{
    public interface IBackgroundManager
    {
        ObservableCollection<Background> Backgrounds { get; }

        void SetBackground(Background background);

        Background SelectedBackground { get; }

        event EventHandler OnSelectedBackgroundChanged;
    }
}

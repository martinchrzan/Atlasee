using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VisFileManager.ViewModelContracts.DetailsPanel;

namespace VisFileManager.Messenger.Messages
{
    public sealed class DetailsItemHoveredMessage
    {
        public static DetailsItemHoveredMessage Create(IDetailsItemViewModel detailsItemViewModel, Point absolutePosition)
        {
            return new DetailsItemHoveredMessage(detailsItemViewModel, absolutePosition);
        }

        private DetailsItemHoveredMessage(IDetailsItemViewModel detailsItemViewModel, Point absolutePosition)
        {
            DetailsItemViewModel = detailsItemViewModel;
            AbsolutePosition = absolutePosition;
        }

        public IDetailsItemViewModel DetailsItemViewModel { get; }

        public Point AbsolutePosition { get; }
    }
}

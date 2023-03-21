using System.Collections.Generic;

namespace VisFileManager.Messenger.Messages
{
    public class DragAndDropMessage
    {
        public DragAndDropMessage(bool isCopyOperation, IEnumerable<string> itemsToDrop)
        {
            IsCopyOperation = isCopyOperation;
            ItemsToDrop = itemsToDrop;
        }

        public bool IsCopyOperation { get; }

        public IEnumerable<string> ItemsToDrop { get; }
    }
}

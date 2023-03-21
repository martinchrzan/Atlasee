using VisFileManager.Enums;

namespace VisFileManager.Messenger.Messages
{
    public sealed class SortingRequest
    {
        public static SortingRequest Create(SortingDirection direction, SortingField field)
        {
            return new SortingRequest(direction, field);
        }

        private SortingRequest(SortingDirection direction, SortingField field)
        {
            SortingDirection = direction;
            SortingField = field;
        }
        public SortingDirection SortingDirection { get; }

        public SortingField SortingField { get; }
    }
}

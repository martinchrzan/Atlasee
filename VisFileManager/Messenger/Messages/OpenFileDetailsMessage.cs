using VisFileManager.Validators;

namespace VisFileManager.Messenger.Messages
{
    public class OpenFileDetailsMessage
    {
        public OpenFileDetailsMessage(FormattedPath formattedPath, bool isSearchResult)
        {
            FormattedPath = formattedPath;
            IsSearchResult = isSearchResult;
        }

        public FormattedPath FormattedPath { get; }

        public bool IsSearchResult { get; }
    }
}

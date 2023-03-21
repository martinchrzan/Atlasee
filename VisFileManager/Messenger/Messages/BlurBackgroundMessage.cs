namespace VisFileManager.Messenger.Messages
{
    public class BlurBackgroundMessage
    {
        public BlurBackgroundMessage(bool enableBlur, bool clickOnWillRemoveBlur)
        {
            EnableBlur = enableBlur;
            ClickOnWillRemoveBlur = clickOnWillRemoveBlur;
        }

        public bool EnableBlur { get; }

        public bool ClickOnWillRemoveBlur { get; }
    }
}

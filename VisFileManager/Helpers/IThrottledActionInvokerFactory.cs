namespace VisFileManager.Helpers
{
    public interface IThrottledActionInvokerFactory
    {
        IThrottledActionInvoker CreateThrottledActionInvoker();
    }
}

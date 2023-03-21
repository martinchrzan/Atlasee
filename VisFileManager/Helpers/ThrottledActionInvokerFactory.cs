using System.ComponentModel.Composition;

namespace VisFileManager.Helpers
{
    [Export(typeof(IThrottledActionInvokerFactory))]
    public class ThrottledActionInvokerFactory : IThrottledActionInvokerFactory
    {
        public IThrottledActionInvoker CreateThrottledActionInvoker()
        {
            return new ThrottledActionInvoker();
        }
    }
}

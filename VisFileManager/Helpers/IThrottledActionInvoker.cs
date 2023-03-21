using System;

namespace VisFileManager.Helpers
{
    public interface IThrottledActionInvoker : IDisposable
    {
        // provided action will be triggered in a given time if there is no
        void ScheduleAction(Action action, int miliseconds);
    }
}

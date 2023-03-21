using System;

namespace VisFileManager.Helpers
{
    public interface IDispatcherTaskScheduler
    {
        void ScheduleAction(Action action, int millisecondsFromNow);
    }
}

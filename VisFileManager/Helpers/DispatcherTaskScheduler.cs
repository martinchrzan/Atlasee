using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;

namespace VisFileManager.Helpers
{
    [Export(typeof(IDispatcherTaskScheduler))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DispatcherTaskScheduler : IDispatcherTaskScheduler
    {
        public void ScheduleAction(Action action, int millisecondsFromNow)
        {
            TimerCallback callback = (state) => {
                Application.Current.Dispatcher.Invoke(() =>
                action.Invoke());

               ((Timer)state).Dispose();
            };
            var timer = new Timer(callback);
            timer.Change(millisecondsFromNow, Timeout.Infinite);  
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisFileManager.Helpers
{
    public static class TaskHelper
    {
        public static async Task PutTaskDelay(int msDelay)
        {
            await Task.Delay(msDelay);
        }
    }
}

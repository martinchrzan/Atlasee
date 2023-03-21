using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisFileManager.Helpers;

namespace VisFileManager.Messenger.Messages
{
    public class EyetrackingTriggersMessage
    {
        public EyetrackingTriggersMessage(EyetrackingTriggerKey keyToChange)
        {
            TriggerKeyToChange = keyToChange;
        }

        public EyetrackingTriggerKey TriggerKeyToChange { get; }
    }
}

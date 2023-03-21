using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tobii.Interaction;
using Tobii.Interaction.Wpf;
using VisFileManager.Shared;

namespace VisFileManager
{
    // dirty hack to access internal private property in tobii assembly to clean up stuff which is creating memory leaks, should be handled on their side!
    public sealed class EyetrackerMemoryLeakCleanerHack : IDisposable
    {
        private const int TimeIntervalInMs = 30000;
        private const string DeletedWpfInteractorsField = "_deletedWpfInteractors";
        private readonly EyetrackerManager _eyetrackerManager;
        private Timer _timer;
        
        public EyetrackerMemoryLeakCleanerHack(EyetrackerManager eyetrackerManager)
        {
            _eyetrackerManager = eyetrackerManager;
            _timer = new Timer(CleanMemory, null, TimeIntervalInMs, Timeout.Infinite);
        }

        private void CleanMemory(object state)
        {
            try
            {
                var fields = new List<FieldInfo>();
                FindFields(fields, typeof(WpfInteractorAgent));
                var inter = fields.First(it => it.FieldType == typeof(WpfInteractorProvider<WpfInteractor>));
                var deletedInteractors = (Dictionary<string, WpfInteractor>)inter.FieldType.GetField(DeletedWpfInteractorsField, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(inter.GetValue(_eyetrackerManager.WpfInteractorAgent));
                if(deletedInteractors.Count > 0)
                {
                    deletedInteractors.Clear();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            catch(Exception ex)
            {
                Logger.LogError("Failed to clean memory leaks in Tobii assembly " + ex.Message);
            }
            
            _timer.Change(TimeIntervalInMs, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private static void FindFields(ICollection<FieldInfo> fields, Type t)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (var field in t.GetFields(flags))
            {
                // Ignore inherited fields.
                if (field.DeclaringType == t)
                    fields.Add(field);
            }

            var baseType = t.BaseType;
            if (baseType != null)
                FindFields(fields, baseType);
        }
    }
}

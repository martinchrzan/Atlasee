using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace VisFileManager.Helpers
{
    public static class WpfUtilities
    {
        public static void SetTracing()
        {
            SetTracing(SourceLevels.Warning, null);
        }

        public static void SetTracing(SourceLevels levels, TraceListener listener)
        {
            if (listener == null)
            {
                listener = new DefaultTraceListener();
            }

            // enable WPF tracing
            PresentationTraceSources.Refresh();

            // enable all WPF Trace sources (change this if you only want DataBindingSource)
            foreach (PropertyInfo pi in typeof(PresentationTraceSources).GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                if (typeof(TraceSource).IsAssignableFrom(pi.PropertyType))
                {
                    TraceSource ts = (TraceSource)pi.GetValue(null, null);
                    ts.Listeners.Add(listener);
                    ts.Switch.Level = levels;
                }
            }
        }

        public static void FocusMainWindow()
        {
            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(App.Current.MainWindow, App.Current.MainWindow);
        }

        public static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}

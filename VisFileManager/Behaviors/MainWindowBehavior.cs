using Microsoft.Xaml.Behaviors;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Helpers;

namespace VisFileManager.Behaviors
{
    public class MainWindowBehavior : Behavior<Window>
    {
        private const int StartupDelayMs = 3000;
        private IGlobalFileManager _globalFileManager;

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
            _globalFileManager = Bootstraper.Container.GetExportedValue<IGlobalFileManager>();
#if DEBUG
            WpfUtilities.SetTracing();
#endif
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            DetachEvents();
        }

        protected override void OnDetaching()
        {
            DetachEvents();
            base.OnDetaching();
        }

        private void DetachEvents()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            _globalFileManager.CurrentPathChanged -= GlobalFileManager_CurrentPathChanged;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _globalFileManager.CurrentPathChanged += GlobalFileManager_CurrentPathChanged;
            ////await TaskHelper.PutTaskDelay(StartupDelayMs);

            //var windowHelper = new WindowInteropHelper(AssociatedObject);

            //var accent = new AccentPolicy { AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND };

            //var accentStructSize = Marshal.SizeOf(accent);

            //var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            //Marshal.StructureToPtr(accent, accentPtr, false);

            //var data = new WindowCompositionAttributeData
            //{
            //    Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
            //    SizeOfData = accentStructSize,
            //    Data = accentPtr
            //};

            //SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            //Marshal.FreeHGlobal(accentPtr);
        }

        private void GlobalFileManager_CurrentPathChanged(object sender, bool e)
        {
            var currentPath = _globalFileManager.CurrentPath;
            if (currentPath.PathType == Validators.PathValidator.PathType.Directory)
            {
                AssociatedObject.Title = currentPath.Name;
            }
            else if (currentPath.PathType == Validators.PathValidator.PathType.MyComputer)
            {
                AssociatedObject.Title = "My Computer";
            }
            else if (currentPath.PathType == Validators.PathValidator.PathType.Drive)
            {
                AssociatedObject.Title = currentPath.Name;
            }
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 1,
            ACCENT_ENABLE_GRADIENT = 0,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }
    }
}

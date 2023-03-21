using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Tobii.Interaction.Wpf;
using VisFileManager.Common;
using VisFileManager.Helpers;

namespace VisFileManager.Behaviors
{
    public class ActivationBehavior : Behavior<FrameworkElement>
    {
        DispatcherTimer _activationDelayedTimer;
        protected bool _hasDelayedActivation;
        private object _activationLock = new object();
        protected ActivationInfoProvider _activationInfoProvider;

        public static readonly DependencyProperty UseTemplatedParentProperty = DependencyProperty.Register(
            "UseTemplatedParent", typeof(bool), typeof(ActivationBehavior));

        public bool UseTemplatedParent
        {
            get { return (bool)GetValue(UseTemplatedParentProperty); }
            set { SetValue(UseTemplatedParentProperty, value); }
        }

        public static readonly DependencyProperty FocusListBoxItemParentOnActivationProperty = DependencyProperty.Register(
            "FocusListBoxItemParentOnActivation", typeof(bool), typeof(ActivationBehavior));

        public bool FocusListBoxItemParentOnActivation
        {
            get { return (bool)GetValue(FocusListBoxItemParentOnActivationProperty); }
            set { SetValue(FocusListBoxItemParentOnActivationProperty, value); }
        }


        public static readonly DependencyProperty PrimaryKeyCommandProperty = DependencyProperty.Register(
            "PrimaryKeyCommand", typeof(ICommand), typeof(ActivationBehavior));

        public ICommand PrimaryKeyCommand
        {
            get { return (ICommand)GetValue(PrimaryKeyCommandProperty); }
            set { SetValue(PrimaryKeyCommandProperty, value); }
        }

        public static readonly DependencyProperty SecondaryKeyCommandProperty = DependencyProperty.Register(
            "SecondaryKeyCommand", typeof(ICommand), typeof(ActivationBehavior));

        public ICommand SecondaryKeyCommand
        {
            get { return (ICommand)GetValue(SecondaryKeyCommandProperty); }
            set { SetValue(SecondaryKeyCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveKeyCommandProperty = DependencyProperty.Register(
           "RemoveKeyCommand", typeof(ICommand), typeof(ActivationBehavior));

        public ICommand RemoveKeyCommand
        {
            get { return (ICommand)GetValue(RemoveKeyCommandProperty); }
            set { SetValue(RemoveKeyCommandProperty, value); }
        }

        public static readonly DependencyProperty ActivationTimeoutInMsProperty = DependencyProperty.Register(
            "ActivationTimeInMs", typeof(int), typeof(ActivationBehavior));

        public int ActivationTimeInMs
        {
            get { return (int)GetValue(ActivationTimeoutInMsProperty); }
            set { SetValue(ActivationTimeoutInMsProperty, value); }
        }

        protected override void OnAttached()
        {
            _activationDelayedTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(ActivationTimeInMs) };

            _activationInfoProvider = Bootstraper.Container.GetExportedValue<ActivationInfoProvider>();

            _activationDelayedTimer.Tick += ActivationDelayedTimer_Tick;
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
            GetTemplatedParent().SetActivatedCommand(new RelayCommand(Activated));
            GetTemplatedParent().AddActivationFocusChangedHandler(ActivationFocusHandler);
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            GetTemplatedParent().SetActivatedCommand(null);
            GetTemplatedParent().RemoveActivationFocusChangedHandler(ActivationFocusHandler);

            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }

        private void ActivationDelayedTimer_Tick(object sender, EventArgs e)
        {
            lock (_activationLock)
            {
                _hasDelayedActivation = true;
                GetTemplatedParent().SetHasGaze(true);
            }
        }

        private void ActivationFocusHandler(object sender, ActivationFocusChangedRoutedEventArgs e)
        {
            if (e.HasTentativeActivationFocus)
            {
                _activationDelayedTimer.Start();
            }
            else if (!e.HasActivationFocus && !e.HasTentativeActivationFocus)
            {
                _hasDelayedActivation = false;
                _activationDelayedTimer.Stop();
                GetTemplatedParent().SetHasGaze(false);
            }
        }

        // Some controls using this template might be inside control template therefore we need to access their templated parent parent - such as border inside ListBoxItem in Template property
        private FrameworkElement GetTemplatedParent()
        {
            if (UseTemplatedParent)
            {
                return AssociatedObject.TemplatedParent as FrameworkElement;
            }
            return AssociatedObject;
        }

        protected override void OnDetaching()
        {
            _hasDelayedActivation = false;
            _activationDelayedTimer.Stop();
            _activationDelayedTimer.Tick -= ActivationDelayedTimer_Tick;
            GetTemplatedParent().RemoveActivationFocusChangedHandler(ActivationFocusHandler);
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            base.OnDetaching();
        }

        private void Activated()
        {
            if (_hasDelayedActivation)
            {
                switch (_activationInfoProvider.LastActivationKey)
                {
                    case EyetrackingTriggerKey.Primary:
                        if (PrimaryKeyCommand != null)
                        {
                            PrimaryKeyCommand.Execute(null);
                            if (FocusListBoxItemParentOnActivation)
                            {
                                FocusListBoxItem();
                            }
                        }
                        break;
                    case EyetrackingTriggerKey.Secondary:
                        if (SecondaryKeyCommand != null)
                        {
                            SecondaryKeyCommand.Execute(null);
                        }
                        break;
                    case EyetrackingTriggerKey.Delete:
                        if (RemoveKeyCommand != null)
                        {
                            RemoveKeyCommand.Execute(null);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void FocusListBoxItem()
        {
            var parent = AssociatedObject.TemplatedParent as FrameworkElement;

            while (parent != null && !(parent is ListBoxItem))
            {
                parent = parent.TemplatedParent as FrameworkElement;
            }

            if (parent is ListBoxItem)
            {
                parent.Focus();
            }
        }
    }
}

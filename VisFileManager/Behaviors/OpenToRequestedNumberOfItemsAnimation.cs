using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace VisFileManager.Behaviors
{
    public class OpenToRequestedNumberOfItemsAnimation : Behavior<FrameworkElement>
    {
        private bool _opened;

        protected override void OnAttached()
        {
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        public TextBlock MoreItemsTextBlock
        {
            get { return (TextBlock)GetValue(MoreItemsTextBlockProperty); }
            set { SetValue(MoreItemsTextBlockProperty, value); }
        }

        public static readonly DependencyProperty MoreItemsTextBlockProperty = DependencyProperty.Register(
            "MoreItemsTextBlock", typeof(TextBlock), typeof(OpenToRequestedNumberOfItemsAnimation));

        public ItemsControl ItemsControl
        {
            get { return (ItemsControl)GetValue(ItemsControlProperty); }
            set { SetValue(ItemsControlProperty, value); }
        }

        public static readonly DependencyProperty ItemsControlProperty = DependencyProperty.Register(
            "ItemsControl", typeof(ItemsControl), typeof(OpenToRequestedNumberOfItemsAnimation));

        public Border BorderWithItems
        {
            get { return (Border)GetValue(BorderWithItemsProperty); }
            set { SetValue(BorderWithItemsProperty, value); }
        }

        public static readonly DependencyProperty BorderWithItemsProperty = DependencyProperty.Register(
            "BorderWithItems", typeof(Border), typeof(OpenToRequestedNumberOfItemsAnimation));

        public int ItemHeight
        {
            get { return (int)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            "ItemHeight", typeof(int), typeof(OpenToRequestedNumberOfItemsAnimation));

        public int MaxNumberOfVisibleItems
        {
            get { return (int)GetValue(MaxNumberOfVisibleItemsProperty); }
            set { SetValue(MaxNumberOfVisibleItemsProperty, value); }
        }

        public static readonly DependencyProperty MaxNumberOfVisibleItemsProperty = DependencyProperty.Register(
            "MaxNumberOfVisibleItems", typeof(int), typeof(OpenToRequestedNumberOfItemsAnimation));

        public IEnumerable<object> Items
        {
            get { return (IEnumerable<object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items", typeof(IEnumerable<object>), typeof(OpenToRequestedNumberOfItemsAnimation));

        private void AssociatedObject_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Animate(false);
        }

        private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Animate(true);
        }

        private void Animate(bool open)
        {
            if ((open && _opened) || (!open && !_opened)) return;

            var height = 0;
            if (open)
            {
                var itemsCount = Items.Count();
                if (MaxNumberOfVisibleItems < itemsCount)
                {
                    ItemsControl.MaxHeight = MaxNumberOfVisibleItems * ItemHeight;
                    // max number of items + 1 to show more 
                    height = MaxNumberOfVisibleItems * ItemHeight + ItemHeight;
                    MoreItemsTextBlock.Visibility = Visibility.Visible;
                    MoreItemsTextBlock.Text = "+ " + (itemsCount - MaxNumberOfVisibleItems) + " more items";
                }
                else
                {
                    height = ItemHeight * itemsCount;
                    ItemsControl.MaxHeight = height;
                    MoreItemsTextBlock.Visibility = Visibility.Collapsed;
                }
            }

            DoubleAnimation openAnimation = new DoubleAnimation(height, new Duration(TimeSpan.FromMilliseconds(100)));

            openAnimation.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };

            BorderWithItems.BeginAnimation(Border.HeightProperty, openAnimation);
            _opened = open;
        }

    }
}

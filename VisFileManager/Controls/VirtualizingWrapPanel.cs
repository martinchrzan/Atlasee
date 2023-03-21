using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace VisFileManager.Controls
{
    // class from: https://github.com/samueldjack/VirtualCollection/blob/master/VirtualCollection/VirtualCollection/VirtualizingWrapPanel.cs
    // MakeVisible() method from: http://www.switchonthecode.com/tutorials/wpf-tutorial-implementing-iscrollinfo
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        private const double ScrollLineAmount = 16.0;

        private Size _extentSize;
        private Size _viewportSize;
        private Point _offset;
        private ItemsControl _itemsControl;
        private readonly Dictionary<UIElement, Rect> _childLayouts = new Dictionary<UIElement, Rect>();

        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register("ItemWidth", typeof(double), typeof(VirtualizingWrapPanel), new PropertyMetadata(1.0, HandleItemDimensionChanged));

        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register("ItemHeight", typeof(double), typeof(VirtualizingWrapPanel), new PropertyMetadata(1.0, HandleItemDimensionChanged));

        private static readonly DependencyProperty VirtualItemIndexProperty =
            DependencyProperty.RegisterAttached("VirtualItemIndex", typeof(int), typeof(VirtualizingWrapPanel), new PropertyMetadata(-1));

        private IRecyclingItemContainerGenerator _itemsGenerator;

        private bool _isInMeasure;

        private static int GetVirtualItemIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(VirtualItemIndexProperty);
        }

        private static void SetVirtualItemIndex(DependencyObject obj, int value)
        {
            obj.SetValue(VirtualItemIndexProperty, value);
        }

        #region Orientation

        /// <summary>
        /// Gets and sets the orientation of the panel.
        /// </summary>
        /// <value>The orientation of the panel.</value>
        public Orientation Orientation
        {
            get
            {
                return (Orientation)GetValue(OrientationProperty);
            }
            set
            {
                SetValue(OrientationProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        /// <remarks>
        /// Returns: The identifier for the Orientation dependency property.
        /// </remarks>
        public static readonly DependencyProperty OrientationProperty = StackPanel.OrientationProperty.AddOwner(typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Orientation.Horizontal));

        #endregion Orientation

        public double ItemHeight
        {
            get
            {
                return (double)GetValue(ItemHeightProperty);
            }
            set
            {
                SetValue(ItemHeightProperty, value);
            }
        }

        public double ItemWidth
        {
            get
            {
                return (double)GetValue(ItemWidthProperty);
            }
            set
            {
                SetValue(ItemWidthProperty, value);
            }
        }

        public VirtualizingWrapPanel()
        {
            //if (!DesignerProperties.GetIsInDesignMode(this))
            //{
            //    Dispatcher.Invoke((Action)Initialize);
            //}
            this.Loaded += VirtualizingWrapPanel_Loaded;
        }

        private void VirtualizingWrapPanel_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke((Action)Initialize);
        }

        private void Initialize()
        {
            var necessaryChidrenTouch = this.Children;
            _itemsControl = ItemsControl.GetItemsOwner(this);
            SetVirtualizationMode(_itemsControl, VirtualizationMode.Recycling);
            // hack to ensure that ItemContainerGenerator is not null - it happens sometimes when switching styles for it's parent
            
            _itemsGenerator = (IRecyclingItemContainerGenerator)ItemContainerGenerator;
            
            InvalidateMeasure();
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);

            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_itemsControl == null)
            {
                return availableSize;
            }

            _isInMeasure = true;
            _childLayouts.Clear();

            var extentInfo = GetExtentInfo(availableSize);


            EnsureScrollOffsetIsWithinConstrains(extentInfo);

            var layoutInfo = GetLayoutInfo(availableSize, (Orientation == Orientation.Vertical) ? ItemHeight : ItemWidth, extentInfo);
         
            // Determine where the first item is in relation to previously realized items
            var generatorStartPosition = _itemsGenerator.GeneratorPositionFromIndex(layoutInfo.FirstRealizedItemIndex);

            var visualIndex = 0;

            Double currentX = layoutInfo.FirstRealizedItemLeft;
            Double currentY = layoutInfo.FirstRealizedLineTop;

            using (_itemsGenerator.StartAt(generatorStartPosition, GeneratorDirection.Forward, true))
            {
                for (var itemIndex = layoutInfo.FirstRealizedItemIndex; itemIndex <= layoutInfo.LastRealizedItemIndex; itemIndex++, visualIndex++)
                {
                    bool newlyRealized;
               
                    var child = (UIElement)_itemsGenerator.GenerateNext(out newlyRealized);
                    if (child == null)
                    {
                        continue;
                    }
                    SetVirtualItemIndex(child, itemIndex);

                    if (newlyRealized)
                    {
                        InsertInternalChild(visualIndex, child);
                    }
                    else
                    {
                        // check if item needs to be moved into a new position in the Children collection
                        if (visualIndex < Children.Count)
                        {
                            if (Children[visualIndex] != child)
                            {
                                var childCurrentIndex = Children.IndexOf(child);

                                if (childCurrentIndex >= 0)
                                {
                                    RemoveInternalChildRange(childCurrentIndex, 1);
                                }

                                InsertInternalChild(visualIndex, child);
                            }
                        }
                        else
                        {
                            // we know that the child can't already be in the children collection
                            // because we've been inserting children in correct visualIndex order,
                            // and this child has a visualIndex greater than the Children.Count
                            AddInternalChild(child);
                        }
                    }

                    // only prepare the item once it has been added to the visual tree
                    _itemsGenerator.PrepareItemContainer(child);

                    child.Measure(new Size(ItemWidth, ItemHeight));

                    _childLayouts.Add(child, new Rect(currentX, currentY, ItemWidth, ItemHeight));

                    if (Orientation == Orientation.Vertical)
                    {
                        if (currentX + ItemWidth * 2 > availableSize.Width)
                        {
                            // wrap to a new line
                            currentY += ItemHeight;
                            currentX = 0;
                        }
                        else
                        {
                            currentX += ItemWidth;
                        }
                    }
                    else
                    {
                        if (currentY + ItemHeight * 2 > availableSize.Height)
                        {
                            // wrap to a new column
                            currentX += ItemWidth;
                            currentY = 0;
                        }
                        else
                        {
                            currentY += ItemHeight;
                        }
                    }
                }
            }

            UpdateScrollInfo(availableSize, extentInfo);

            var desiredSize = new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
                                       double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);

            _isInMeasure = false;

            return desiredSize;
        }

        private void EnsureScrollOffsetIsWithinConstrains(ExtentInfo extentInfo)
        {
            if (Orientation == Orientation.Vertical)
            {
                _offset.Y = Clamp(_offset.Y, 0, extentInfo.MaxVerticalOffset);
            }
            else
            {
                _offset.X = Clamp(_offset.X, 0, extentInfo.MaxHorizontalOffset);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in Children)
            {
                if (_childLayouts.ContainsKey(child))
                {
                    child.Arrange(_childLayouts[child]);
                }
                else
                {
                    child.Arrange(new Rect(new Point(-500, -500), new Size(ItemWidth, ItemHeight)));
                }
            }
            return finalSize;
        }

        private void UpdateScrollInfo(Size availableSize, ExtentInfo extentInfo)
        {
            _viewportSize = availableSize;
            if (Orientation == Orientation.Vertical)
            {
                _extentSize = new Size(availableSize.Width, extentInfo.ExtentHeight);
            }
            else
            {
                _extentSize = new Size(extentInfo.ExtentWidth, availableSize.Height);
            }

            InvalidateScrollInfo();
        }

        private ItemLayoutInfo GetLayoutInfo(Size availableSize, double itemHeightOrWidth, ExtentInfo extentInfo)
        {
            if (_itemsControl == null)
            {
                return new ItemLayoutInfo();
            }

            // we need to ensure that there is one realized item prior to the first visible item, and one after the last visible item,
            // so that keyboard navigation works properly. For example, when focus is on the first visible item, and the user
            // navigates up, the ListBox selects the previous item, and the scrolls that into view - and this triggers the loading of the rest of the items
            // in that row

            if (Orientation == Orientation.Vertical)
            {
                var firstVisibleLine = (int)Math.Floor(VerticalOffset / itemHeightOrWidth);

                var firstRealizedIndex = Math.Max(extentInfo.ItemsPerLine * firstVisibleLine - 1, 0);
                var firstRealizedItemLeft = firstRealizedIndex % extentInfo.ItemsPerLine * ItemWidth - HorizontalOffset;
                var firstRealizedItemTop = (firstRealizedIndex / extentInfo.ItemsPerLine) * itemHeightOrWidth - VerticalOffset;

                var firstCompleteLineTop = (firstVisibleLine == 0 ? firstRealizedItemTop : firstRealizedItemTop + ItemHeight);
                var completeRealizedLines = (int)Math.Ceiling((availableSize.Height - firstCompleteLineTop) / itemHeightOrWidth);

                var lastRealizedIndex = Math.Min(firstRealizedIndex + completeRealizedLines * extentInfo.ItemsPerLine + 2, _itemsControl.Items.Count - 1);

                return new ItemLayoutInfo
                {
                    FirstRealizedItemIndex = firstRealizedIndex,
                    FirstRealizedItemLeft = firstRealizedItemLeft,
                    FirstRealizedLineTop = firstRealizedItemTop,
                    LastRealizedItemIndex = lastRealizedIndex,
                };
            }
            else
            {
                var firstVisibleColumn = (int)Math.Floor(HorizontalOffset / itemHeightOrWidth);

                var firstRealizedIndex = Math.Max(extentInfo.ItemsPerColumn * firstVisibleColumn - 1, 0);
                var firstRealizedItemTop = firstRealizedIndex % extentInfo.ItemsPerColumn * ItemHeight - VerticalOffset;
                var firstRealizedItemLeft = (firstRealizedIndex / extentInfo.ItemsPerColumn) * itemHeightOrWidth - HorizontalOffset;

                var firstCompleteColumnLeft = (firstVisibleColumn == 0 ? firstRealizedItemLeft : firstRealizedItemLeft + ItemWidth);
                var completeRealizedColumns = (int)Math.Ceiling((availableSize.Width - firstCompleteColumnLeft) / itemHeightOrWidth);

                var lastRealizedIndex = Math.Min(firstRealizedIndex + completeRealizedColumns * extentInfo.ItemsPerColumn + 2, _itemsControl.Items.Count - 1);

                return new ItemLayoutInfo
                {
                    FirstRealizedItemIndex = firstRealizedIndex,
                    FirstRealizedItemLeft = firstRealizedItemLeft,
                    FirstRealizedLineTop = firstRealizedItemTop,
                    LastRealizedItemIndex = lastRealizedIndex,
                };
            }
        }

        private ExtentInfo GetExtentInfo(Size viewPortSize)
        {
            if (_itemsControl == null)
            {
                return new ExtentInfo();
            }

            if (Orientation == Orientation.Vertical)
            {
                var itemsPerLine = Math.Max((int)Math.Floor(viewPortSize.Width / ItemWidth), 1);
                var totalLines = (int)Math.Ceiling((double)_itemsControl.Items.Count / itemsPerLine);
                var extentHeight = Math.Max(totalLines * ItemHeight, viewPortSize.Height);

                return new ExtentInfo
                {
                    ItemsPerLine = itemsPerLine,
                    TotalLines = totalLines,
                    ExtentHeight = extentHeight,
                    MaxVerticalOffset = extentHeight - viewPortSize.Height,
                };
            }
            else
            {
                var itemsPerColumn = Math.Max((int)Math.Floor(viewPortSize.Height / ItemHeight), 1);
                var totalColumns = (int)Math.Ceiling((double)_itemsControl.Items.Count / itemsPerColumn);
                var extentWidth = Math.Max(totalColumns * ItemWidth, viewPortSize.Width);

                return new ExtentInfo
                {
                    ItemsPerColumn = itemsPerColumn,
                    TotalColumns = totalColumns,
                    ExtentWidth = extentWidth,
                    MaxHorizontalOffset = extentWidth - viewPortSize.Width
                };
            }
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - ScrollLineAmount);
        }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + ScrollLineAmount);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ScrollLineAmount);
        }

        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + ScrollLineAmount);
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - ViewportHeight);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + ViewportHeight);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ItemWidth);
        }

        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + ItemWidth);
        }

        public void MouseWheelUp()
        {
            if (Orientation == Orientation.Vertical)
            {
                SetVerticalOffset(VerticalOffset - ScrollLineAmount * SystemParameters.WheelScrollLines);
            }
            else
            {
                MouseWheelLeft();
            }
        }

        public void MouseWheelDown()
        {
            if (Orientation == Orientation.Vertical)
            {
                SetVerticalOffset(VerticalOffset + ScrollLineAmount * SystemParameters.WheelScrollLines);
            }
            else
            {
                MouseWheelRight();
            }
        }

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ScrollLineAmount * SystemParameters.WheelScrollLines);
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + ScrollLineAmount * SystemParameters.WheelScrollLines);
        }

        public void SetHorizontalOffset(double offset)
        {
            if (_isInMeasure)
            {
                return;
            }

            offset = Clamp(offset, 0, ExtentWidth - ViewportWidth);
            _offset = new Point(offset, _offset.Y);

            InvalidateScrollInfo();
            InvalidateMeasure();
        }

        public void SetVerticalOffset(double offset)
        {
            if (_isInMeasure)
            {
                return;
            }

            offset = Clamp(offset, 0, ExtentHeight - ViewportHeight);
            _offset = new Point(_offset.X, offset);

            InvalidateScrollInfo();
            InvalidateMeasure();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            if (rectangle.IsEmpty ||
                visual == null ||
                visual == this ||
                !IsAncestorOf(visual))
            {
                return Rect.Empty;
            }

            rectangle = visual.TransformToAncestor(this).TransformBounds(rectangle);

            var viewRect = new Rect(HorizontalOffset, VerticalOffset, ViewportWidth, ViewportHeight);
            rectangle.X += viewRect.X;
            rectangle.Y += viewRect.Y;

            viewRect.X = CalculateNewScrollOffset(viewRect.Left, viewRect.Right, rectangle.Left, rectangle.Right);
            viewRect.Y = CalculateNewScrollOffset(viewRect.Top, viewRect.Bottom, rectangle.Top, rectangle.Bottom);

            SetHorizontalOffset(viewRect.X);
            SetVerticalOffset(viewRect.Y);
            rectangle.Intersect(viewRect);

            rectangle.X -= viewRect.X;
            rectangle.Y -= viewRect.Y;

            return rectangle;
        }

        private static double CalculateNewScrollOffset(double topView, double bottomView, double topChild, double bottomChild)
        {
            var offBottom = topChild < topView && bottomChild < bottomView;
            var offTop = bottomChild > bottomView && topChild > topView;
            var tooLarge = (bottomChild - topChild) > (bottomView - topView);

            if (!offBottom && !offTop)
                return topView;

            if ((offBottom && !tooLarge) || (offTop && tooLarge))
                return topChild;

            return bottomChild - (bottomView - topView);
        }

        public ItemLayoutInfo GetVisibleItemsRange()
        {
            return GetLayoutInfo(_viewportSize, (Orientation == Orientation.Vertical) ? ItemHeight : ItemWidth, GetExtentInfo(_viewportSize));
        }

        protected override void BringIndexIntoView(int index)
        {
            if (Orientation == Orientation.Vertical)
            {
                var currentVisibleMin = _offset.Y;
                var currentVisibleMax = _offset.Y + _viewportSize.Height - ItemHeight;
                var itemsPerLine = Math.Max((int)Math.Floor(_viewportSize.Width / ItemWidth), 1);

                var verticalOffsetRequiredToPutItemAtTopRow = Math.Floor((double)index / itemsPerLine) * ItemHeight;

                if (verticalOffsetRequiredToPutItemAtTopRow < currentVisibleMin) // if item is above visible area put it on the top row
                    SetVerticalOffset(verticalOffsetRequiredToPutItemAtTopRow);
                else if (verticalOffsetRequiredToPutItemAtTopRow > currentVisibleMax) // if item is below visible area move to put it on bottom row
                    SetVerticalOffset(verticalOffsetRequiredToPutItemAtTopRow - _viewportSize.Height + ItemHeight);
            }
            else
            {
                var currentVisibleMin = _offset.X;
                var currentVisibleMax = _offset.X + _viewportSize.Width - ItemWidth;
                var itemsPerColumn = Math.Max((int)Math.Floor(_viewportSize.Height / ItemHeight), 1);

                var horizontalOffsetRequiredToPutItemAtLeftRow = Math.Floor((double)index / itemsPerColumn) * ItemWidth;

                if (horizontalOffsetRequiredToPutItemAtLeftRow < currentVisibleMin) // if item is left from the visible area put it at the left most column
                    SetHorizontalOffset(horizontalOffsetRequiredToPutItemAtLeftRow);
                else if (horizontalOffsetRequiredToPutItemAtLeftRow > currentVisibleMax) // if item is right from the visible area put it at the right most column
                    SetHorizontalOffset(horizontalOffsetRequiredToPutItemAtLeftRow - _viewportSize.Width + ItemWidth);
            }
        }

        public bool CanVerticallyScroll
        {
            get;
            set;
        }

        public bool CanHorizontallyScroll
        {
            get;
            set;
        }

        public double ExtentWidth
        {
            get
            {
                return _extentSize.Width;
            }
        }

        public double ExtentHeight
        {
            get
            {
                return _extentSize.Height;
            }
        }

        public double ViewportWidth
        {
            get
            {
                return _viewportSize.Width;
            }
        }

        public double ViewportHeight
        {
            get
            {
                return _viewportSize.Height;
            }
        }

        public double HorizontalOffset
        {
            get
            {
                return _offset.X;
            }
        }

        public double VerticalOffset
        {
            get
            {
                return _offset.Y;
            }
        }

        public ScrollViewer ScrollOwner
        {
            get;
            set;
        }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
            {
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        private static void HandleItemDimensionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wrapPanel = (d as VirtualizingWrapPanel);

            if (wrapPanel != null)
                wrapPanel.InvalidateMeasure();
        }

        private double Clamp(double value, double min, double max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        internal class ExtentInfo
        {
            public int ItemsPerLine;
            public Int32 ItemsPerColumn;
            public int TotalLines;
            public Int32 TotalColumns;
            public double ExtentHeight;
            public double ExtentWidth;
            public double MaxVerticalOffset;
            public double MaxHorizontalOffset;
        }

        public class ItemLayoutInfo
        {
            public int FirstRealizedItemIndex;
            public double FirstRealizedLineTop;
            public double FirstRealizedItemLeft;
            public int LastRealizedItemIndex;
        }
    }
}

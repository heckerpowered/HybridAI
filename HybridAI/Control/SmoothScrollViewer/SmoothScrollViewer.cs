using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace HybridAI.Control
{
    internal partial class SmoothScrollViewer : ScrollViewer
    {
        private static double GetScrollEndPoint(double scrollStepRatio, double scrollPositionRatio, ScrollDirection scrollDirection)
        {
            if (scrollDirection is ScrollDirection.Down or ScrollDirection.Right)
            {
                var to = scrollPositionRatio + scrollStepRatio;
                return to > 0.95 ? 1.0 : to;
            }
            else if (scrollDirection is ScrollDirection.Up or ScrollDirection.Left)
            {
                var to = scrollPositionRatio - scrollStepRatio;
                return to < 0.05 ? 0.0 : to;
            }

            throw new ArgumentException("Invalid scroll direction", nameof(scrollDirection));
        }

        public SmoothScrollViewer()
        {
        }
        public double VerticalScrollRatio
        {
            get { return (double)GetValue(VerticalScrollRatioProperty); }
            set { SetValue(VerticalScrollRatioProperty, value); }
        }

        public double HorizontalScrollRatio
        {
            get { return (double)GetValue(HorizontalScrollRatioProperty); }
            set { SetValue(HorizontalScrollRatioProperty, value); }
        }

        public void SmoothScroll(double scrollStepRatio, double scrollPositionRatio, ScrollDirection scrollDirection)
        {
            if (double.IsNaN(scrollStepRatio) || double.IsNaN(scrollPositionRatio))
            {
                return;
            }

            var scrollAnimation = new DoubleAnimation
            {
                From = scrollPositionRatio,
                To = GetScrollEndPoint(scrollStepRatio, scrollPositionRatio, scrollDirection),
                Duration = TimeSpan.FromMilliseconds(150)
            };

            Storyboard.SetTarget(scrollAnimation, this);

            var storyboard = new Storyboard();
            storyboard.Children.Add(scrollAnimation);

            if (scrollDirection is ScrollDirection.Down or ScrollDirection.Up)
            {
                Storyboard.SetTargetProperty(scrollAnimation, new PropertyPath(VerticalScrollRatioProperty));
            }
            else if (scrollDirection is ScrollDirection.Right or ScrollDirection.Left)
            {
                Storyboard.SetTargetProperty(scrollAnimation, new PropertyPath(HorizontalScrollRatioProperty));
            }

            storyboard.Begin();
        }

        public void SmoothScrollToEnd()
        {
            var scrollStepRatio = ViewportHeight / (ExtentHeight - ViewportHeight);
            var scrollPositionRatio = ContentVerticalOffset / ScrollableHeight;
            SmoothScroll(scrollStepRatio, scrollPositionRatio, ScrollDirection.Down);
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            var scrollStepRatio = ViewportHeight / (ExtentHeight - ViewportHeight);
            var scrollPositionRatio = ContentVerticalOffset / ScrollableHeight;

            if (e.VerticalChange < 0)
            {
                SmoothScroll(scrollStepRatio, scrollPositionRatio, ScrollDirection.Up);
            }
            else if (e.VerticalChange > 0)
            {
                SmoothScroll(scrollStepRatio, scrollPositionRatio, ScrollDirection.Down);
            }
        }
    }
}

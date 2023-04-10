using System.Windows;
using System.Windows.Controls;

namespace HybridAI.Control
{
    internal partial class SmoothScrollViewer
    {
        private static void HorizontalScrollRatioChangedCallBack(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var scrollViewer = dependencyObject as ScrollViewer;
            scrollViewer?.ScrollToHorizontalOffset((double)(dependencyPropertyChangedEventArgs.NewValue) * scrollViewer.ScrollableWidth);
        }
        private static void VerticalScrollRatioChangedCallBack(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var scrollViewer = dependencyObject as ScrollViewer;
            scrollViewer?.ScrollToVerticalOffset((double)dependencyPropertyChangedEventArgs.NewValue * scrollViewer.ScrollableHeight);
        }
    }
}

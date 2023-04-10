using System.Windows;

namespace HybridAI.Control
{
    internal partial class SmoothScrollViewer
    {
        public static readonly DependencyProperty VerticalScrollRatioProperty = DependencyProperty.Register(
    "VerticalScrollRatio",
    typeof(double),
    typeof(SmoothScrollViewer),
    new PropertyMetadata(0.0, new PropertyChangedCallback(VerticalScrollRatioChangedCallBack)));

        public static readonly DependencyProperty HorizontalScrollRatioProperty = DependencyProperty.Register(
            "HorizontalScrollRatio",
            typeof(double),
            typeof(SmoothScrollViewer),
            new PropertyMetadata(0.0, new PropertyChangedCallback(HorizontalScrollRatioChangedCallBack)));
    }
}

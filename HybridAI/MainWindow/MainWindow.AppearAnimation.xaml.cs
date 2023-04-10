using System;
using System.Windows.Media.Animation;

namespace HybridAI
{
    public partial class MainWindow
    {
        private static DoubleAnimation AppearOpacityAnimation { get; } = new DoubleAnimation()
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(150)
        };

        private static DoubleAnimation AppearBlurAnimation { get; } = new DoubleAnimation()
        {
            From = 20,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(150)
        };

        private static DoubleAnimation AppearScaleXAnimation { get; } = new DoubleAnimation()
        {
            From = 2,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(150)
        };

        private static DoubleAnimation AppearScaleYAnimation { get; } = new DoubleAnimation()
        {
            From = 2,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(150)
        };

        private static Storyboard AppearStoryboard { get; } = new Storyboard()
        {
            Duration = TimeSpan.FromMilliseconds(150)
        };
    }
}
using System;
using System.Windows.Media.Animation;

namespace HybridAI
{
    public partial class MainWindow
    {
        private static DoubleAnimation DisappearOpacityAnimation { get; } = new DoubleAnimation()
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(150)
        };

        private static DoubleAnimation DisappearBlurAnimation { get; } = new DoubleAnimation()
        {
            From = 0,
            To = 20,
            Duration = TimeSpan.FromMilliseconds(150)
        };

        private static DoubleAnimation DisappearScaleXAnimation { get; } = new DoubleAnimation()
        {
            From = 1,
            To = 2,
            Duration = TimeSpan.FromMilliseconds(150)
        };

        private static DoubleAnimation DisappearScaleYAnimation { get; } = new DoubleAnimation()
        {
            From = 1,
            To = 2,
            Duration = TimeSpan.FromMilliseconds(150)
        };

        private static Storyboard DisappearStoryboard { get; } = new Storyboard()
        {
            Duration = TimeSpan.FromMilliseconds(150)
        };
    }
}

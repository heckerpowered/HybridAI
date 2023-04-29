using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HybridAI
{
    public partial class MainWindow
    {
        public void EndInitialize()
        {
            MainGrid.IsEnabled = true;
            (GlobalGrid.FindResource("EndInitialize") as Storyboard)?.Begin();
        }

        public void BeginInitialize()
        {
            MainGrid.IsEnabled = false;
            (GlobalGrid.FindResource("BeginInitialize") as Storyboard)?.Begin();
        }

        public async void OpenOptionsPage()
        {
            MainGrid.IsEnabled = false;
            OptionsPage.IsEnabled = true;
            (GlobalGrid.FindResource("OpenOptionsPage") as Storyboard)?.Begin();

            await Task.Delay(150);
            if (!MainGrid.IsEnabled)
            {
                MainGrid.Visibility = Visibility.Collapsed;
            }
        }

        public void CloseOptionsPage()
        {
            MainGrid.Visibility = Visibility.Visible;
            MainGrid.IsEnabled = true;
            OptionsPage.IsEnabled = false;
            (GlobalGrid.FindResource("CloseOptionsPage") as Storyboard)?.Begin();
        }

        public static void PerformDisappearAnimation(FrameworkElement frameworkElement)
        {
            frameworkElement.IsEnabled = false;

            if (DisappearStoryboard.Children.Count == 0)
            {
                Storyboard.SetTargetProperty(DisappearOpacityAnimation, new PropertyPath(OpacityProperty));
                Storyboard.SetTargetProperty(DisappearBlurAnimation, new PropertyPath("(UIElement.Effect).(BlurEffect.Radius)"));
                Storyboard.SetTargetProperty(DisappearScaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
                Storyboard.SetTargetProperty(DisappearScaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

                DisappearStoryboard.Children.Add(DisappearOpacityAnimation);
                DisappearStoryboard.Children.Add(DisappearBlurAnimation);
                DisappearStoryboard.Children.Add(DisappearScaleXAnimation);
                DisappearStoryboard.Children.Add(DisappearScaleYAnimation);
            }

            if (frameworkElement.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.CenterX = frameworkElement.ActualWidth / 2;
                scaleTransform.CenterY = frameworkElement.ActualHeight / 2;
            }

            DisappearStoryboard.Begin(frameworkElement);
        }

        public static void PerformAppearAnimation(FrameworkElement frameworkElement)
        {
            frameworkElement.IsEnabled = true;

            if (AppearStoryboard.Children.Count == 0)
            {
                Storyboard.SetTargetProperty(AppearOpacityAnimation, new PropertyPath(OpacityProperty));
                Storyboard.SetTargetProperty(AppearBlurAnimation, new PropertyPath("(UIElement.Effect).(BlurEffect.Radius)"));
                Storyboard.SetTargetProperty(AppearScaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
                Storyboard.SetTargetProperty(AppearScaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

                AppearStoryboard.Children.Add(AppearOpacityAnimation);
                AppearStoryboard.Children.Add(AppearBlurAnimation);
                AppearStoryboard.Children.Add(AppearScaleXAnimation);
                AppearStoryboard.Children.Add(AppearScaleYAnimation);
            }

            if (frameworkElement.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.CenterX = frameworkElement.ActualWidth / 2;
                scaleTransform.CenterY = frameworkElement.ActualHeight / 2;
            }

            AppearStoryboard.Begin(frameworkElement);
        }

        public static void QueueWorkWithAnimation(FrameworkElement frameworkElement, Action action)
        {
            PerformDisappearAnimation(frameworkElement);
            action();
            PerformAppearAnimation(frameworkElement);
        }
    }
}

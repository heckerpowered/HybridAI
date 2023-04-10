using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HybridAI
{
    public partial class MainWindow
    {
        public void BeginRefresh()
        {
            (MainGrid.FindResource("BeginRefresh") as Storyboard)?.Begin();
            RefreshButton.IsEnabled = false;

            CurrentChatHistory = null;
        }

        public void EndRefresh()
        {
            (MainGrid.FindResource("EndRefresh") as Storyboard)?.Begin();
            RefreshButton.IsEnabled = true;
        }

        public void BeginRequest()
        {
            (MainGrid.FindResource("BeginRequest") as Storyboard)?.Begin();
            SendMessageButton.IsEnabled = false;
        }

        public void EndRequest()
        {
            (MainGrid.FindResource("EndRequest") as Storyboard)?.Begin();
            SendMessageButton.IsEnabled = true;
        }

        public void EndInitialize()
        {
            MainGrid.IsEnabled = true;
            ((Content as Grid)?.FindResource("LoadComplete") as Storyboard)?.Begin();
        }

        public static void PlayDisappearAnimation(FrameworkElement element)
        {
            element.IsEnabled = false;

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

            if (element.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.CenterX = element.ActualWidth / 2;
                scaleTransform.CenterY = element.ActualHeight / 2;
            }

            DisappearStoryboard.Begin(element);
        }

        public static void PlayAppearAnimation(FrameworkElement element)
        {
            element.IsEnabled = true;

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

            if (element.RenderTransform is ScaleTransform scaleTransform)
            {
                scaleTransform.CenterX = element.ActualWidth / 2;
                scaleTransform.CenterY = element.ActualHeight / 2;
            }

            AppearStoryboard.Begin(element);
        }

        public static void QueueWorkWithAnimation(FrameworkElement element, Action action)
        {
            PlayDisappearAnimation(element);
            action();
            PlayAppearAnimation(element);
        }
    }
}

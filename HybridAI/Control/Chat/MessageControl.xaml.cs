using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using HybridAI.AI;

namespace HybridAI.Control.Chat
{
    /// <summary>
    /// Message.xaml 的交互逻辑
    /// </summary>
    public partial class MessageControl : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MessageControl));
        private readonly MessageKind messageKind;
        private readonly CancellationTokenSource? cancellationTokenSource;
        private readonly MainWindow? mainWindow;
        public MessageControl()
        {
            InitializeComponent();
        }

        private Task? AnimationPerformance { get; set; }

        public MessageControl(MessageBuilder builder) : this()
        {
            var text = builder.text;
            var performAnimation = builder.performAnimation;
            var foreground = builder.foreground;
            var messageKind = builder.kind;
            var cancellationTokenSource = builder.cancellationTokenSource;
            var container = builder.container;

            Text = text;
            this.messageKind = messageKind;
            this.cancellationTokenSource = cancellationTokenSource;
            mainWindow = container;
            if (foreground != null)
            {
                Foreground = foreground;
                animatedTextBlock.Foreground = foreground;
            }

            if (performAnimation)
            {
                AnimationPerformance = Task.Run(async () => await PerformAnimation(text));
            }
            else
            {
                animatedTextBlock.Text = text;
            }
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set
            {
                SetValue(TextProperty, value);
                animatedTextBlock.Text = value;
            }
        }

        public async Task PerformAnimation(string text)
        {
            if (AnimationPerformance != null && !AnimationPerformance.IsCompleted)
            {
                await AnimationPerformance;
            }

            AnimationPerformance = Core(text);
            await AnimationPerformance;

            async Task Core(string text)
            {
                foreach (char character in text)
                {
                    await Dispatcher.BeginInvoke(async () => await animatedTextBlock.AddString(character.ToString()));
                    await Task.Delay(10);
                }
            }
        }

        public Task? GetAnimationPerformance()
        {
            return AnimationPerformance;
        }

        public async Task AddString(string text)
        {
            await animatedTextBlock.AddString(text);
        }

        private async void Retry(object sender, RoutedEventArgs e)
        {
            if (mainWindow == null || mainWindow.Requesting)
            {
                return;
            }

            var container = mainWindow.MessageContainer.Items;
            var index = container.IndexOf(this);
            if (index == -1 || index >= container.Count)
            {
                Trace.TraceWarning("Attempt to retry, but no relevant message was found in the message container");
                return;
            }

            if (!mainWindow.Requesting)
            {
                mainWindow.BeginRequest();
            }

            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }

            MainWindow.PerformDisappearAnimation((FrameworkElement)Content);
            await Task.Delay(150);

            container.RemoveAt(index);
            var isUserMessage = messageKind is MessageKind.UserMessage;
            if (!isUserMessage)
            {
                --index;
            }

            var control = container[index];
            MainWindow.PerformDisappearAnimation((FrameworkElement)((MessageControl)control).Content);
            await Task.Delay(150);
            container.RemoveAt(index);

            var input = isUserMessage ? Text : ((MessageControl)control).Text;

            var context = new ChatContext(mainWindow, input);
            var request = new MessageRequest(input, Guid.NewGuid().ToString());
            var discontinuousMessageReceiver = context.GetDiscontinuousMessageReceiver();
            var exceptionHandler = context.GetExceptionHandler();
            var cancellationToken = context.CancellationToken;

            // try/except blocks cannot catch exceptions for asynchronous methods
            await Task.Run(async () => await Server.RequestAIStream(request, discontinuousMessageReceiver, exceptionHandler, cancellationToken));

            var chatContext = mainWindow.GetSelectedChatHistory().ChatContext;
            var chatContextIndex = index / 2;
            if (chatContextIndex >= chatContext.Count)
            {
                chatContextIndex = chatContext.Count - 1;
            }
            if (chatContextIndex < 0)
            {
                chatContextIndex = 0;
            }

            chatContext.RemoveAt(chatContextIndex);
        }

        private void RetryButtonLoaded(object sender, RoutedEventArgs e)
        {
            var retrySupported = mainWindow != null && !mainWindow.Requesting;
            var menuItem = (MenuItem)sender;
            menuItem.IsEnabled = retrySupported;
        }

        private void CancelButtonLoaded(object sender, RoutedEventArgs e)
        {
            var cancellationSupported = cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested;
            var menuItem = (MenuItem)sender;
            menuItem.IsEnabled = !cancellationSupported;
        }

        private void Copy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(animatedTextBlock.Text);
        }
    }
}

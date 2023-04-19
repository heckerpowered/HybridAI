using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

using HybridAI.AI;
using HybridAI.Control.Chat;

namespace HybridAI
{
    public class ChatContext
    {
        private readonly StringBuilder receivedMessageBuilder = new();
        private readonly int messageControlPosition;
        private readonly MainWindow window;
        private readonly string input;
        private readonly CancellationTokenSource cancellationTokenSource = new();

        private MessageControl? responseControl;
        private bool waitControlRemoved;
        private bool messageReceived;

        public ChatContext(MainWindow window, string input)
        {
            this.window = window;
            this.input = input;

            var messageBuilder = new MessageBuilder().SetText(input).SetMessageKind(MessageKind.UserMessage).SetContainer(window);
            var messageControl = new MessageControl(messageBuilder);
            messageControlPosition = window.MessageContainer.Items.Add(messageControl);
            window.MessageContainer.Items.Add(new WaitingResponseControl(this));
            window.MessageContainerScrollViewer.SmoothScrollToEnd();
        }

        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        public DiscontinuousMessageReceiver GetDiscontinuousMessageReceiver()
        {
            return message => window.Dispatcher.Invoke(() => ReceiveMessage(message));
        }

        public ExceptionHandler GetExceptionHandler()
        {
            return exception => window.Dispatcher.Invoke(() => ReportException(exception));
        }

        private async Task ReceiveMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                window.GetSelectedChatHistory().ChatContext.Add(new(input, receivedMessageBuilder.ToString()));
                window.EndRequest();
                return;
            }

            receivedMessageBuilder.Append(message);

            if (CheckIfFirstTimeReceiveMessage() || responseControl == null)
            {
                responseControl = new(new MessageBuilder().SetContainer(window).SetCancellationTokenSource(cancellationTokenSource).SetMessageKind(MessageKind.ResponseMessage))
                {
                    Foreground = (Brush)window.FindResource("ResponseForegroundColor")
                };

                RemoveWaitControl();
                window.MessageContainer.Items.Add(responseControl);
            }

            await responseControl.PerformAnimation(message);
            window.MessageContainerScrollViewer.SmoothScrollToEnd();
        }

        private bool CheckIfFirstTimeReceiveMessage()
        {
            if (messageReceived)
            {
                return false;
            }
            messageReceived = true;

            return true;
        }

        public void RemoveWaitControl()
        {
            if (waitControlRemoved)
            {
                return;
            }
            waitControlRemoved = true;

            window.MessageContainer.Items.RemoveAt(messageControlPosition + 1);
        }

        public void ReportException(Exception exception)
        {
            RemoveWaitControl();

            responseControl = new(new MessageBuilder().SetContainer(window).SetMessageKind(MessageKind.ErrorMessage))
            {
                Foreground = Brushes.Red,
                Text = exception.ToString()
            };

            window.MessageContainer.Items.Add(responseControl);

            Trace.TraceError("An error occurred during requesting AI:");
            Trace.Indent();
            Trace.WriteLine(exception.ToString());
            Trace.Unindent();

            Trace.WriteLine("User input:");
            Trace.Indent();
            Trace.WriteLine(input);
            Trace.Unindent();

            var message = receivedMessageBuilder.ToString();
            if (string.IsNullOrWhiteSpace(message))
            {
                responseControl.AddString(Environment.NewLine);
                Trace.WriteLine("No message received from AI");
            }
            else
            {
                Trace.WriteLine("A partial response from the AI was received before the error occurred. The request may have been interrupted due to network reasons.");
                Trace.WriteLine("Received message from AI:");
                Trace.Indent();
                Trace.WriteLine(receivedMessageBuilder.ToString());
                Trace.Unindent();
            }

            window.MessageContainerScrollViewer.SmoothScrollToEnd();

            window.EndRequest();
        }

        public async Task Interrupt()
        {
            cancellationTokenSource.Cancel();
            await RemoveLastControl();
            await RemoveLastControl();

            async Task RemoveLastControl()
            {
                var frameworkElement = (FrameworkElement)window.MessageContainer.Items[^1];
                frameworkElement.Effect = new BlurEffect();
                frameworkElement.RenderTransform = new ScaleTransform();

                MainWindow.PerformDisappearAnimation(frameworkElement);

                await Task.Delay(150);

                window.MessageContainer.Items.RemoveAt(window.MessageContainer.Items.Count - 1);

            }

            window.EndRequest();
        }
    }

    public class ChatDisplay
    {
        public ChatDisplay(MainWindow mainWindow, FrameworkElement currentDisplayElement)
        {
            MainWindow = mainWindow;
            CurrentDisplayElement = currentDisplayElement;

            SetDisplayElement(currentDisplayElement);
        }

        public MainWindow MainWindow { get; }
        public FrameworkElement CurrentDisplayElement { get; private set; }

        public ItemCollection ItemCollection => MainWindow.MessageContainer.Items;

        private void SetDisplayElement(FrameworkElement frameworkElement)
        {
            ItemCollection.RemoveAt(ItemCollection.Count - 1);
            ItemCollection.Add(frameworkElement);
        }
    }
}

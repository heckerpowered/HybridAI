using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

using HybridAI.AI;
using HybridAI.Control.Chat;

namespace HybridAI
{
    public class ChatContext
    {
        private readonly StringBuilder messageBuilder = new();
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
            messageControlPosition = window.MessageContainer.Items.Add(new MessageControl(input, false));
            window.MessageContainer.Items.Add(new WaitingResponseControl());

            CancellationToken.Register(() =>
            {
                RemoveWaitControl();
                window.MessageContainer.Items.RemoveAt(window.MessageContainer.Items.Count - 1);
                window.EndRequest();
            });
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
            if (string.IsNullOrWhiteSpace(message))
            {
                window.GetSelectedChatHistory().ChatContext.Add(new(input, messageBuilder.ToString()));
                window.EndRequest();
                return;
            }

            messageBuilder.Append(message);
            CheckIfFirstTimeReceiveMessage();

            responseControl ??= new()
            {
                Foreground = (Brush)window.FindResource("ResponseForegroundColor")
            };

            await responseControl.PerformAnimation(message);
            window.MessageContainerScrollViewer.SmoothScrollToEnd();

            void CheckIfFirstTimeReceiveMessage()
            {
                if (!messageReceived)
                {
                    responseControl = new()
                    {
                        Foreground = (Brush)window.FindResource("ResponseForegroundColor")
                    };

                    RemoveWaitControl();
                    window.MessageContainer.Items.Add(responseControl);
                }

                messageReceived = true;
            }
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

            responseControl = new()
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

            var message = messageBuilder.ToString();
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
                Trace.WriteLine(messageBuilder.ToString());
                Trace.Unindent();
            }

            window.MessageContainerScrollViewer.SmoothScrollToEnd();

            window.EndRequest();
        }
    }
}

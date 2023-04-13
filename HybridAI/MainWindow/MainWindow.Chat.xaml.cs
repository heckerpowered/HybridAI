using System;
using System.Text;
using System.Windows.Media;

using HybridAI.AI;
using HybridAI.Control.Chat;

namespace HybridAI
{
    public partial class MainWindow
    {
        public class ChatContext
        {
            private readonly StringBuilder stringBuilder = new();
            private readonly int messageControlPosition;
            private readonly MainWindow window;
            private readonly string input;

            private MessageControl? responseControl;
            private bool waitControlRemoved;
            private bool messageReceived;

            public ChatContext(MainWindow window, string input)
            {
                this.window = window;
                this.input = input;
                messageControlPosition = window.MessageContainer.Items.Add(new MessageControl(input, false));
                window.MessageContainer.Items.Add(new WaitingResponseControl());
            }

            public DiscontinuousMessageReceiver GetDiscontinuousMessageReceiver()
            {
                return message => window.Dispatcher.Invoke(() => ReceiveMessage(message));
            }

            public ExceptionHandler GetExceptionHandler()
            {
                return exception => window.Dispatcher.Invoke(() => ReportException(exception));
            }

            private void ReceiveMessage(string message)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    window.GetSelectedChatHistory().ChatContext.Add(new(input, stringBuilder.ToString()));
                    return;
                }

                stringBuilder.Append(message);
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
                responseControl?.PlayAnimation(message);

                window.MessageContainerScrollViewer.SmoothScrollToEnd();
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

                responseControl ??= new();
                responseControl.Foreground = Brushes.Red;

                window.MessageContainer.Items.Add(responseControl);

                responseControl?.AddString(Environment.NewLine);
                responseControl?.AddString(exception.ToString());

                window.MessageContainerScrollViewer.SmoothScrollToEnd();
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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

            private async Task ReceiveMessage(string message)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    window.GetSelectedChatHistory().ChatContext.Add(new(input, stringBuilder.ToString()));
                    window.EndRequest();
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

                responseControl ??= new()
                {
                    Foreground = (Brush)window.FindResource("ResponseForegroundColor")
                };

                messageReceived = true;
                await responseControl.PlayAnimation(message);

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

                var message = stringBuilder.ToString();
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
                    Trace.WriteLine(stringBuilder.ToString());
                    Trace.Unindent();
                }

                window.MessageContainerScrollViewer.SmoothScrollToEnd();

                window.EndRequest();
            }
        }
    }
}

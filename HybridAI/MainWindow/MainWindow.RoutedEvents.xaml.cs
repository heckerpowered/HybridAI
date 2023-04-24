using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

using HybridAI.AI;
using HybridAI.Control.Chat;
using HybridAI.History;

namespace HybridAI
{
    public partial class MainWindow
    {
        private ChatHistory? CurrentChatHistory { get; set; } = null;
        private CancellationTokenSource LoadingTaskCancellationTokenSource { get; } = new();
        private bool LoadingChatHistory { get; set; }
        internal bool Requesting { get; private set; } = false;

        /// <summary>
        /// Request AI with the message typed in the text box, the request will not be processed if the text box is empty
        /// or there is already a request being processed.
        /// </summary>
        private async void SendCurrentTypedMessage()
        {
            var messageToSent = Message.Text.Trim();
            if (Requesting || string.IsNullOrWhiteSpace(messageToSent))
            {
                return;
            }

            if (ChatHistoryList.Items.Count == 0)
            {
                CreateNewChat(messageToSent);
            }

            BeginRequest();

            var context = new ChatContext(this, messageToSent);
            var request = new MessageRequest(messageToSent, Guid.NewGuid().ToString());
            var discontinuousMessageReceiver = context.GetDiscontinuousMessageReceiver();
            var exceptionHandler = context.GetExceptionHandler();
            var cancellationToken = context.CancellationToken;

            // try/except blocks cannot catch exceptions for asynchronous methods
            await Task.Run(async () => await Server.RequestAIStream(request, discontinuousMessageReceiver, exceptionHandler, cancellationToken));
        }

        /// <summary>
        /// End request AI, mark not requesting, continue processing requests for AI, refreshing chat history, and switching chat history.
        /// send button, refresh button, and create new chat button will be enable and shown. Appear animation will be played for these buttons.
        /// <paramref name="endSuccessfully">This parameter affects the animation of the send message button.
        /// If the request ends successfully, the button will fly in from the left. Other wise it will appear in the form of jitter.</paramref>
        /// </summary>
        internal void EndRequest(bool endSuccessfully = true)
        {
            Requesting = false;
            ChatHistoryList.IsEnabled = true;

            var resourceKey = endSuccessfully ? "EndRequest" : "Error";
            ((Content as Grid)?.FindResource(resourceKey) as Storyboard)?.Begin();
            PerformAppearAnimation(RefreshButton);
            PerformAppearAnimation(CreateNewChatButton);


        }

        /// <summary>
        /// Begin request AI, mark requesting, no more requests for AI, refreshing chat history, or switching chat history will be processed,
        /// send button, refresh button, and create new chat button will be disable and hidden. Disappear animation will be played for these buttons.
        /// </summary>
        internal void BeginRequest()
        {
            Requesting = true;
            Message.Text = string.Empty;
            ChatHistoryList.IsEnabled = false;

            // PerformDisappearAnimation(SendMessageButton);
            ((Content as Grid)?.FindResource("SendMessage") as Storyboard)?.Begin();
            PerformDisappearAnimation(RefreshButton);
            PerformDisappearAnimation(CreateNewChatButton);
        }

        /// <summary>
        /// Add a message control to current UI, does not affect any <c>ChatHistory</c>.
        /// Do not call this method to add message control after requested AI. This method is
        /// used to add message control when loading <c>ChatHistory</c> to the UI.
        /// </summary>
        /// <param name="message">The message the user input</param>
        private void AddMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var messageBuilder = new MessageBuilder().SetText(message).SetMessageKind(MessageKind.UserMessage).SetContainer(this);
            var messageControl = new MessageControl(messageBuilder);
            MessageContainer.Items.Add(messageControl);
        }

        /// <summary>
        /// Add a response control to current UI, does not affect any <c>ChatHistory</c>.
        /// Do not call this method to add response control after requested AI. This method is
        /// used to add message control when loading <c>ChatHistory</c> to the UI.
        /// </summary>
        /// <param name="message">The response message provided by AI</param>
        private void AddResponse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var foreground = (Brush)FindResource("ResponseForegroundColor");
            var builder = new MessageBuilder().SetText(message).SetMessageKind(MessageKind.ResponseMessage).SetForeground(foreground).SetContainer(this);
            var messageControl = new MessageControl(builder);
            MessageContainer.Items.Add(messageControl);
        }

        /// <summary>
        /// Get the <c>ChatHistory</c> at the index selected in the user control(<c>ChatHistoryList</c>),
        /// returns a new one if no <c>ChatHistory</c> is selected.
        /// </summary>
        /// <returns><c>ChatHistory</c> at the specified index or a new one if no <c>ChatHistory</c> is selected.</returns>
        internal ChatHistory GetSelectedChatHistory() => GetChatHistory(ChatHistoryList.SelectedIndex);

        /// <summary>
        /// Get <c>ChatHistory</c> at the specified index, returns a new one if the index is out of range.
        /// </summary>
        /// <param name="index"></param>
        /// <returns><c>ChatHistory</c> at the specified index or a new one if the index of out of range.</returns>
        private ChatHistory GetChatHistory(int index)
        {
            if (AllChatHistory.Count == 0)
            {
                AllChatHistory.Add(new());
            }

            if (index < 0)
            {
                return AllChatHistory.First();
            }
            if (index > AllChatHistory.Count - 1)
            {
                return AllChatHistory.Last();
            }

            return AllChatHistory[index];
        }

        /// <summary>
        /// Put the specified <c>ChatHistory</c> to the UI, the process id delayed by the animation and can be interrupted.
        /// </summary>
        /// <param name="history"><c>ChatHistory</c> to be put into the UI</param>
        /// <returns>The loading task delayed by animation, exactly the return value of <c>Task.Delay</c></returns>
        private async Task PutChatHistoryToUI(ChatHistory history)
        {
            if (CurrentChatHistory == history)
            {
                return;
            }

            LoadingChatHistory = true;
            CurrentChatHistory = history;
            MessageContainer.Items.Clear();

            try
            {
                foreach (var message in history.ChatContext)
                {
                    await AddMessageAndWait(message.Input);
                    await AddResponseAndWait(message.Response);
                }

                MessageContainerScrollViewer.SmoothScrollToEnd();
            }
            catch (TaskCanceledException)
            {
                LoadingTaskCancellationTokenSource.TryReset();
            }

            LoadingChatHistory = false;

            async Task AddMessageAndWait(string message)
            {
                AddMessage(message);
                await Task.Delay(1, LoadingTaskCancellationTokenSource.Token);
            }

            async Task AddResponseAndWait(string message)
            {
                AddResponse(message);
                await Task.Delay(1, LoadingTaskCancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Interrupt the process of loading <c>ChatHistory</c> to the UI.
        /// </summary>
        private void InterruptLoading()
        {
            LoadingTaskCancellationTokenSource.Cancel();
        }
    }
}

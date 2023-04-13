using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private bool Requesting { get; set; } = false;
        private async void SendCurrentTypedMessage()
        {
            var messageToSent = Message.Text.Trim();
            if (Requesting || string.IsNullOrWhiteSpace(messageToSent))
            {
                return;
            }

            BeginRequest();

            var context = new ChatContext(this, messageToSent);
            var request = new MessageRequest(messageToSent, Guid.NewGuid().ToString());
            var discontinuousMessageReceiver = context.GetDiscontinuousMessageReceiver();
            var exceptionHandler = context.GetExceptionHandler();

            // try/except blocks cannot catch exceptions for asynchronous methods
            await Task.Run(() => Server.RequestAIStream(request, discontinuousMessageReceiver, exceptionHandler));
            EndRequest();
        }

        private void EndRequest()
        {
            Requesting = false;
            ChatHistoryList.IsEnabled = true;

            PlayAppearAnimation(SendMessageButton);
            PlayAppearAnimation(RefreshButton);
            PlayAppearAnimation(CreateNewChatButton);
        }

        private void BeginRequest()
        {
            Requesting = true;
            Message.Text = string.Empty;
            ChatHistoryList.IsEnabled = false;

            PlayDisappearAnimation(SendMessageButton);
            PlayDisappearAnimation(RefreshButton);
            PlayDisappearAnimation(CreateNewChatButton);
        }

        private void AddMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            MessageContainer.Items.Add(new MessageControl(message, false));
        }

        private void AddResponse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            MessageContainer.Items.Add(new ResponseControl(message));
        }

        private void AddError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            MessageContainer.Items.Add(new ErrorControl(message));
        }

        private ChatHistory GetSelectedChatHistory()
        {
            return GetChatHistory(ChatHistoryList.SelectedIndex);
        }

        private ChatHistory GetChatHistory(int index)
        {
            if (index == -1)
            {
                return AllChatHistory.First();
            }
            if (index > AllChatHistory.Count - 1)
            {
                return AllChatHistory.Last();
            }

            return AllChatHistory[index];
        }

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
    }
}

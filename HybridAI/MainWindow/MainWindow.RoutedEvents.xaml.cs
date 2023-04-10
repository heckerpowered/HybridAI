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
        private async Task SendCurrentTypedMessage()
        {
            var messageToSent = Message.Text.Trim();
            if (Requesting || string.IsNullOrWhiteSpace(messageToSent))
            {
                return;
            }

            Requesting = true;
            BeginRequest();

            AddMessage(messageToSent);
            MessageContainer.Items.Add(new WaitingResponseControl());

            Message.Text = string.Empty;
            MessageContainerScrollViewer.SmoothScrollToEnd();

            try
            {
                var response = await Server.RequestAI(new(messageToSent));
                MessageContainer.Items.RemoveAt(MessageContainer.Items.Count - 1);
                AddResponse(response);
                GetSelectedChatHistory().ChatContext.Add(new(messageToSent, response));
            }
            catch (Exception e)
            {
                MessageContainer.Items.RemoveAt(MessageContainer.Items.Count - 1);
                AddError(e.ToString());
            }

            Requesting = false;
            EndRequest();
        }

        private void AddMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            MessageContainer.Items.Add(new MessageControl(message));
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

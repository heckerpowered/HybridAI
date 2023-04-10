using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using HybridAI.History;

namespace HybridAI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private List<ChatHistory> AllChatHistory { get; set; } = new();

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            _ = SendCurrentTypedMessage();
        }

        private void OnMessageTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown && e.Key == Key.Enter)
            {
                _ = SendCurrentTypedMessage();
            }
        }

        private void ChatHistorySelected(object sender, SelectionChangedEventArgs e)
        {
            if (LoadingChatHistory)
            {
                LoadingTaskCancellationTokenSource.Cancel();
            }

            _ = PutChatHistoryToUI(GetSelectedChatHistory());
        }

        private void OnContentRendered(object sender, System.EventArgs e)
        {
            Task.Factory.StartNew(Initialize);
        }

        private async void Initialize()
        {
            await LoadAllChatHistory();
            await Dispatcher.BeginInvoke(CompleteLoad);
        }

        private async Task LoadAllChatHistory()
        {
            try
            {
                AllChatHistory = (await Task.Factory.StartNew(ChatHistory.Load)).ToList();
            }
            catch (Exception) { }
        }

        private void CompleteLoad()
        {
            if (AllChatHistory.Count == 0)
            {
                AllChatHistory.Add(new ChatHistory());
            }
            else
            {
                _ = PutChatHistoryToUI(AllChatHistory.First());
                PutAllChatHistoryTitleToUI();
            }

            EndInitialize();
        }

        private void PutAllChatHistoryTitleToUI()
        {
            foreach (var history in AllChatHistory)
            {
                var title = history.ChatContext.First().Input;
                ChatHistoryList.Items.Add(title);
            }
        }

        private void SaveAllChatHistory()
        {
            foreach (var history in AllChatHistory)
            {
                history.Save();
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            SaveAllChatHistory();
        }

        private void OnCreateNewChat(object sender, RoutedEventArgs e)
        {
            var messageToSent = Message.Text.Trim();
            if (string.IsNullOrWhiteSpace(messageToSent))
            {
                Message.Focus();
                return;
            }

            AllChatHistory.Add(new ChatHistory());
            ChatHistoryList.Items.Add(messageToSent);

            ChatHistoryList.SelectedIndex = ChatHistoryList.Items.Count - 1;
            _ = SendCurrentTypedMessage();
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            QueueWorkWithAnimation(RefreshButton, async () =>
            {
                var index = ChatHistoryList.SelectedIndex == -1 ? 0 : ChatHistoryList.SelectedIndex;

                SaveAllChatHistory();
                ChatHistoryList.Items.Clear();
                MessageContainer.Items.Clear();

                await Task.Run(LoadAllChatHistory);
                PutAllChatHistoryTitleToUI();
                await PutChatHistoryToUI(GetChatHistory(index));
            });
        }
    }
}
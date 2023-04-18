using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Trace.TraceInformation("Initializing component");
            var time = DateTime.Now;
            InitializeComponent();
            Trace.TraceInformation($"Component initialized, time cost: {DateTime.Now - time}");
        }

        private List<ChatHistory> AllChatHistory { get; set; } = new();

        /// <summary>
        /// Called when the send message button is clicked, request AI with current typed message.
        /// </summary>
        /// <param name="sender">The button the user clicks</param>
        /// <param name="e">Routed event args</param>
        private void SendMessage(object sender, RoutedEventArgs e)
        {
            SendCurrentTypedMessage();
        }

        /// <summary>
        /// Called when the user down a key, if the key is enter, request AI with current typed message.
        /// </summary>
        /// <param name="sender">The text box focused on when the user down the key</param>
        /// <param name="e">Key event args</param>
        private void OnMessageTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown && e.Key == Key.Enter)
            {
                SendCurrentTypedMessage();
            }
        }

        /// <summary>
        /// Called when the user selected a chat history, interrupt loading if the chat history is loading, put the selected chat history to the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChatHistorySelected(object sender, SelectionChangedEventArgs e)
        {
            if (LoadingChatHistory)
            {
                InterruptLoading();
            }

            _ = PutChatHistoryToUI(GetSelectedChatHistory());
        }

        /// <summary>
        /// Called when the content of the window is rendered, start initialize.
        /// </summary>
        /// <param name="sender">The window of the content rendered</param>
        /// <param name="e">Event args</param>
        private void OnContentRendered(object sender, EventArgs e)
        {
            Trace.TraceInformation("Window rendered, queueing the initialize work to the thread pool.");
            Task.Run(Initialize);
        }

        /// <summary>
        /// Initialize the window, load all chat histories, put the first chat history to the UI, put all chat histories' title to the list box.
        /// </summary>
        private async void Initialize()
        {
            Trace.TraceInformation("Begin initialize from thread pool");
            await LoadAllChatHistory();



            await Dispatcher.BeginInvoke(CompleteLoad);
        }

        /// <summary>
        /// Load all chat history, does not affect the UI.
        /// </summary>
        /// <returns></returns>
        private async Task LoadAllChatHistory()
        {
            Trace.TraceInformation("Loading chat history");
            try
            {
                AllChatHistory = (await Task.Factory.StartNew(ChatHistory.Load)).ToList();
            }
            catch (Exception exception)
            {
                Trace.TraceError("An error occurred during loading chat history:");
                Trace.Indent();
                Trace.WriteLine(exception.ToString());
                Trace.Unindent();
            }
        }

        /// <summary>
        /// Called when the initial loading is completed, put the first chat history to the UI, put all chat histories' title to the list box.
        /// </summary>
        private void CompleteLoad()
        {
            if (AllChatHistory.Count == 0)
            {
                Trace.TraceInformation("No chat history was loaded");
                AllChatHistory.Add(new ChatHistory());
            }
            else
            {
                _ = PutChatHistoryToUI(AllChatHistory.First());
                PutAllChatHistoryTitleToUI();
            }

            EndInitialize();
            Trace.TraceInformation("Loading completed");
        }

        /// <summary>
        /// Put all chat histories' title to the list box.
        /// </summary>
        private void PutAllChatHistoryTitleToUI()
        {
            foreach (var history in AllChatHistory)
            {
                var title = history.ChatContext.First().Input;
                ChatHistoryList.Items.Add(title);
            }
        }

        /// <summary>
        /// Save all the chat history to the disk.
        /// </summary>
        private void SaveAllChatHistory()
        {
            foreach (var history in AllChatHistory)
            {
                history.Save();
            }
        }

        /// <summary>
        /// Called when the window is closed, save the chat history.
        /// </summary>
        /// <param name="sender">The window closed</param>
        /// <param name="e">Event args</param>
        private void OnWindowClosed(object sender, EventArgs e)
        {
            Trace.TraceInformation("Window closed");
            SaveAllChatHistory();
        }

        /// <summary>
        /// Called when the user clicks the create new chat button, request AI with current typed message, 
        /// and create a new chat history, focus on the text box if no message is typed.
        /// </summary>
        /// <param name="sender">The button the user clicks</param>
        /// <param name="e">Routed event args</param>
        private void OnCreateNewChat(object sender, RoutedEventArgs e)
        {
            var messageToSent = Message.Text.Trim();
            if (string.IsNullOrWhiteSpace(messageToSent))
            {
                Message.Focus();
                return;
            }

            CreateNewChat(messageToSent);
            SendCurrentTypedMessage();
        }

        private void CreateNewChat(string title)
        {
            AllChatHistory.Add(new ChatHistory());
            ChatHistoryList.Items.Add(title);

            ChatHistoryList.SelectedIndex = ChatHistoryList.Items.Count - 1;
        }

        /// <summary>
        /// Called when the user clicks the refresh button, refresh the chat history. Chat history will be saved before refresh.
        /// The refresh button is disabled and hidden while refreshing the chat history, disappear animation will be played before refresh 
        /// and appear animation will be played after refresh.
        /// </summary>
        /// <param name="sender">The button the user clicks</param>
        /// <param name="e">Routed event args</param>
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

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Trace.TraceInformation("Stopping");
        }
    }
}
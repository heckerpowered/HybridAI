using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

using HybridAI.History;
using HybridAI.Language;
using HybridAI.Options;
using HybridAI.Pages;
using HybridAI.Security;
using HybridAI.Update;

using MaterialDesignThemes.Wpf;

using Windows.Security.Credentials;
using Windows.Security.Credentials.UI;

namespace HybridAI;

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

        Thread.CurrentThread.Priority = ThreadPriority.Highest;
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

        ChangeLanguage(Properties.Settings.LanguageIndex);
        CheckExplicitEncryption();
        CleanExpiredFiles();
        await LoadAllChatHistory();
        await Dispatcher.BeginInvoke(InitializePropertiesPage);
        await Dispatcher.BeginInvoke(CompleteLoad);
    }

    private void CleanExpiredFiles()
    {
        var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
        foreach (var file in directoryInfo.GetFiles())
        {
            if (file.Extension == ".expired")
            {
                file.Delete();
                Trace.TraceInformation($"Deleting expired file: {file.Name}");
            }
        }
    }

    private void CheckExplicitEncryption()
    {
        if (!Properties.Settings.ExplicitEncryptChatHistory)
        {
            return;
        }

        Trace.TraceInformation("Explicit encrypt chat history is enabled, requesting password.");

        string? password = string.Empty;
        var vault = new PasswordVault();

        try
        {
            var passwordCredential = vault.Retrieve("HybridAI", "HybridAI");
            passwordCredential.RetrievePassword();
            password = passwordCredential.Password;
        }
        catch (Exception exception)
        {
            Trace.TraceError("Failed to retrieve password from password vault");
            Trace.Indent();
            Trace.WriteLine(exception.Message);
            Trace.Unindent();
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            password = Dispatcher.Invoke(RequestPassword);
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            Trace.TraceInformation("Updating encryption descriptor");
            var stopWatch = Stopwatch.StartNew();
            EncryptionManager.EncryptionDescriptor = EncryptionDescriptor.GetEncryptionDescriptor(Encoding.Unicode.GetBytes(password));
            Trace.TraceInformation($"Encryption descriptor updated, time elapsed: {stopWatch.Elapsed}");
        }
    }
    private void InitializePropertiesPage()
    {
        OptionsPage.GoBack += CloseOptionsPage;
        OptionsPage.LanguageChanged += LanguageChanged;
        OptionsPage.ExplicitEncryptionPropertyChanged += ExplicitEncryptionPropertyChanged;
        OptionsPage.SavePassword.BooleanPropertyChanged += SavePasswordPropertyChanged;
        OptionsPage.CheckForUpdate.Click += CheckForUpdate;
        OptionsPage.SaveSettings.Click += SaveSettings;

        OptionsPage.ExplicitEncryption.CheckBox.IsChecked = Properties.Settings.ExplicitEncryptChatHistory;
        OptionsPage.SavePassword.CheckBox.IsChecked = Properties.Settings.SavePassword;
    }

    private async void SaveSettings(object sender, RoutedEventArgs e)
    {
        await Task.Run(Properties.SaveProperties);
        Snackbar.MessageQueue?.Enqueue(Translate("SettingsSaved"));
    }

    private async void CheckForUpdate(object sender, RoutedEventArgs e)
    {
        var element = (ButtonBase)sender;
        ButtonProgressAssist.SetIsIndeterminate(element, true);
        ButtonProgressAssist.SetIsIndicatorVisible(element, true);
        element.IsEnabled = false;

        try
        {
            Trace.TraceInformation("Checking update");
            var expiredFiles = await Task.Run(UpdateChecker.CheckUpdate);
            if (expiredFiles.Count > 0)
            {
                Trace.TraceInformation("Update available");
                Snackbar.MessageQueue?.Enqueue(Translate("UpdateAvailable"), Translate("Update"),
                    () => Update(expiredFiles));
            }
            else
            {
                Trace.TraceInformation("The application is up to date");
                Snackbar.MessageQueue?.Enqueue(Translate("UpToDate"));
            }
        }
        catch (Exception exception)
        {
            Trace.TraceError("Error occurred during checking for update");
            Trace.Indent();
            Trace.WriteLine(exception.ToString());
            Trace.Unindent();

            Snackbar.MessageQueue?.Enqueue(Translate("FailedToUpdate"));
        }

        ButtonProgressAssist.SetIsIndicatorVisible(element, false);
        element.IsEnabled = true;

        async void Update(List<string> expiredFiles)
        {
            element.IsEnabled = false;
            ButtonProgressAssist.SetIsIndicatorVisible(element, true);

            try
            {
                await Task.Run(() => UpdateChecker.Update(expiredFiles, ProgressCallback));
            }
            catch (Exception exception)
            {
                Trace.TraceError("Error occurred during checking for update");
                Trace.Indent();
                Trace.WriteLine(exception.ToString());
                Trace.Unindent();

                Snackbar.MessageQueue?.Enqueue(Translate("FailedToUpdate"));
            }
        }

        void ProgressCallback(double Value)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (ButtonProgressAssist.GetIsIndeterminate(element))
                {
                    ButtonProgressAssist.SetIsIndeterminate(element, false);
                }

                if (Value == 100)
                {
                    ButtonProgressAssist.SetIsIndicatorVisible(element, false);
                    element.IsEnabled = true;

                    Snackbar.MessageQueue?.Enqueue(Translate("Updated"), Translate("Restart"), () =>
                    {
                        Close();

                        Trace.TraceInformation("Restarting");
                        Process.Start("HybridAI.exe");
                    });
                }
                ButtonProgressAssist.SetValue(element, Value);
            });
        }
    }

    private async void SavePasswordPropertyChanged(bool Value)
    {
        OptionsPage.SavePassword.CheckBox.IsEnabled = false;
        if (Value)
        {
            OptionsPage.SavePassword.CheckBox.IsChecked = false;
            var password = await Task.Run(ValidatePassword);
            if (string.IsNullOrWhiteSpace(password))
            {
                OptionsPage.SavePassword.CheckBox.IsEnabled = true;
                return;
            }

            var vault = new PasswordVault();
            vault.Add(new("HybridAI", "HybridAI", password));
            Trace.TraceInformation("Password successfully added to credential locker");

            Properties.Settings.SavePassword = true;
            OptionsPage.SavePassword.CheckBox.IsEnabled = true;
            OptionsPage.SavePassword.CheckBox.IsChecked = true;
        }
        else
        {
            OptionsPage.SavePassword.CheckBox.IsChecked = true;
            if (!await VerifyIdentity())
            {
                OptionsPage.SavePassword.CheckBox.IsEnabled = true;
                return;
            }

            Properties.Settings.SavePassword = false;
            OptionsPage.SavePassword.CheckBox.IsEnabled = true;
            OptionsPage.SavePassword.CheckBox.IsChecked = false;
            RemoveStoredPassword();
        }
    }

    private async void ExplicitEncryptionPropertyChanged(bool Value)
    {
        OptionsPage.ExplicitEncryption.CheckBox.IsEnabled = false;
        if (Value)
        {
            OptionsPage.ExplicitEncryption.CheckBox.IsChecked = false;

            var password = RequestPassword();
            if (string.IsNullOrWhiteSpace(password))
            {
                OptionsPage.ExplicitEncryption.CheckBox.IsEnabled = true;
                return;
            }

            PerformDisappearAnimation(OptionsPage);
            BeginInitialize();

            await Task.Run(() =>
            {
                EncryptionManager.EncryptionDescriptor = EncryptionDescriptor.GetEncryptionDescriptor(Encoding.Unicode.GetBytes(password));
                Properties.Settings.ExplicitEncryptChatHistory = true;
                if (Properties.Settings.SavePassword)
                {
                    var passwordVault = new PasswordVault();
                    passwordVault.Add(new("HybridAI", "HybridAI", password));
                }

                Properties.SaveProperties();
                SaveAllChatHistory();
            });

            OptionsPage.ExplicitEncryption.CheckBox.IsEnabled = true;
            OptionsPage.ExplicitEncryption.CheckBox.IsChecked = true;

            EndInitialize();
            PerformAppearAnimation(OptionsPage);
        }
        else
        {
            OptionsPage.ExplicitEncryption.CheckBox.IsChecked = true;
            string? password = string.Empty;
            if (!await VerifyIdentity())
            {
                OptionsPage.ExplicitEncryption.CheckBox.IsEnabled = true;
                return;
            }

            PerformDisappearAnimation(OptionsPage);
            BeginInitialize();

            await Task.Run(() =>
            {
                RemoveStoredPassword();
                EncryptionManager.EncryptionDescriptor = EncryptionDescriptor.GetEncryptionDescriptor();
                SaveAllChatHistory();
                Properties.Settings.ExplicitEncryptChatHistory = false;
                Properties.SaveProperties();
            });

            EndInitialize();
            PerformAppearAnimation(OptionsPage);
            OptionsPage.ExplicitEncryption.CheckBox.IsEnabled = true;
            OptionsPage.ExplicitEncryption.CheckBox.IsChecked = false;
        }
    }

    private static void RemoveStoredPassword()
    {
        try
        {
            var vault = new PasswordVault();
            var passwordCredential = vault.Retrieve("HybridAI", "HybridAI");
            vault.Remove(passwordCredential);
            Trace.TraceInformation("Password successfully removed from credential locker");
        }
        catch (Exception exception)
        {
            Trace.TraceError("Unable to remove password credential");
            Trace.TraceError("Failed to retrieve password from password vault");
            Trace.Indent();
            Trace.WriteLine(exception.Message);
            Trace.Unindent();
        }
    }

    private async Task<bool> VerifyIdentity()
    {
        if (await UserConsentVerifier.CheckAvailabilityAsync() == UserConsentVerifierAvailability.Available
                && await UserConsentVerifier.RequestVerificationAsync(Translate("ChangeSettingsVerify")) != UserConsentVerificationResult.Verified
                || string.IsNullOrWhiteSpace(await Task.Run(ValidatePassword)))
        {
            Trace.TraceError("Identity verification not passed");
            return false;
        }

        return true;
    }

    private static string Translate(string key)
    {
        return (string)Application.Current.Resources.MergedDictionaries[0][key];
    }

    private async void LanguageChanged(int LanguageIndex)
    {
        PerformDisappearAnimation(OptionsPage);
        BeginInitialize();

        // Await for the animation to complete
        await Task.Delay(500);
        ChangeLanguage(LanguageIndex);

        EndInitialize();
        PerformAppearAnimation(OptionsPage);
    }

    private static void ChangeLanguage(int LanguageIndex)
    {
        var resourceDictionary = new ResourceDictionary
        {
            Source = new Uri(Path.Combine("Language", Languages.LanguageFileNames[LanguageIndex]), UriKind.Relative)
        };

        Application.Current.Resources.MergedDictionaries[0] = resourceDictionary;

        var languageCode = Languages.LanguageCodes[LanguageIndex];
        var cultureInfo = new CultureInfo(languageCode);
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);

        Trace.TraceInformation("Changing language");
        Trace.Indent();
        Trace.WriteLine($"Language index: {LanguageIndex}");
        Trace.WriteLine($"Language name: {Languages.LanguageDisplayNames[LanguageIndex]}");
        Trace.WriteLine($"Language code: 0x{languageCode:X}");
        Trace.Unindent();
    }

    /// <summary>
    /// LoadProperties all chat history, does not affect the UI.
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
            if (history.ChatContext.Count == 0)
            {
                continue;
            }

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
        Properties.SaveProperties();
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
            Trace.TraceInformation("Refreshing");

            SaveAllChatHistory();
            ChatHistoryList.Items.Clear();
            MessageContainer.Items.Clear();

            await Task.Run(LoadAllChatHistory);
            PutAllChatHistoryTitleToUI();
            await PutChatHistoryToUI(GetChatHistory(index));
        });
    }

    private void OnWindowClosing(object sender, CancelEventArgs e)
    {
        Trace.TraceInformation("Stopping");
    }

    private void OpenOptionsPage(object sender, RoutedEventArgs e)
    {
        var scaleTransform = (ScaleTransform)MainGrid.RenderTransform;
        scaleTransform.CenterX = MainGrid.ActualWidth / 2;
        scaleTransform.CenterY = MainGrid.ActualHeight / 2;

        var optionsPageScaleTransform = (ScaleTransform)OptionsPageBorder.RenderTransform;
        optionsPageScaleTransform.CenterX = OptionsPageBorder.ActualWidth / 2;
        optionsPageScaleTransform.CenterY = OptionsPageBorder.ActualHeight / 2;

        OpenOptionsPage();
    }

    private void CloseOptionsPage(object sender, RoutedEventArgs e)
    {
        CloseOptionsPage();
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && OptionsPage.IsEnabled)
        {
            CloseOptionsPage();
        }
    }
}
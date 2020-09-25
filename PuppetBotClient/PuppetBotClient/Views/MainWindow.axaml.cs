using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Discord.Net;
using PuppetBotClient.Discord;
using System;

namespace PuppetBotClient.Views
{
    public class MainWindow : Window
    {
        private DiscordConnectionView DiscordConnectionView { get; }
        private CheckBox PressEnterSendCheckbox { get; }
        private Button SendMessageButton { get; }
        private TextBox MessageTextBox { get; }
        private TextBlock MessageHistoryTextBlock { get; }
        private ScrollViewer MessageHistoryScrollViewer { get; }

        private DiscordManager _discordManager;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            //_discordManager = discordManager;
            _discordManager = new DiscordManager();

            // UI Controls
            DiscordConnectionView = this.Find<DiscordConnectionView>(nameof(DiscordConnectionView));
            PressEnterSendCheckbox = this.Find<CheckBox>(nameof(PressEnterSendCheckbox));
            SendMessageButton = this.Find<Button>(nameof(SendMessageButton));
            MessageTextBox = this.Find<TextBox>(nameof(MessageTextBox));
            MessageHistoryTextBlock = this.Find<TextBlock>(nameof(MessageHistoryTextBlock));
            MessageHistoryScrollViewer = this.Find<ScrollViewer>(nameof(MessageHistoryScrollViewer));

            // Events
            SendMessageButton.Click += SendMessageButton_Clicked;
            MessageTextBox.KeyUp += MessageTextBox_EnterPressed;

            // Discord
            _discordManager.Connected += DiscordManager_Connected;
            _discordManager.Disconnected += DiscordManager_Disconnected;
            _discordManager.UserUpdated += DiscordManager_UserUpdated;

            this.Initialized += MainWindow_Initialized;
            StartDiscordConnection();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #region Discord Events

        private void DiscordManager_Connected()
        {
            Dispatcher.UIThread.InvokeAsync(async () => {
                var currentUser = await _discordManager.GetCurrentDiscordUser();
                var serverSelection = await _discordManager.GetServerSelection();
                DiscordConnectionView.SetConnectionStatus(DiscordConnectionView.ConnectionStatus.Connected);
                DiscordConnectionView.SetUserViewModel(currentUser);
                DiscordConnectionView.SetChannelSelectionModel(serverSelection);
                AddMessageHistory($"Connected as {currentUser.Username}");
            });
        }

        private void DiscordManager_Disconnected(Exception ex)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                DiscordConnectionView.SetConnectionStatus(DiscordConnectionView.ConnectionStatus.Disconnected);
                if (ex != null)
                {
                    AddMessageHistory($"Disconnect Error! ${ex.Message}");
                }
            });
        }

        private void DiscordManager_UserUpdated(ViewModels.Discord.DiscordUserViewModel updatedUser)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                DiscordConnectionView.SetUserViewModel(updatedUser);
            });
        }

        #endregion

        #region UI Events

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            MessageTextBox.Focus();
        }


        
        public void SendMessageButton_Clicked(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        public void MessageTextBox_EnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (e.KeyModifiers == KeyModifiers.Shift)
            {
                return;
            }

            if (!SendMessageButton.IsEnabled)
            {
                return;
            }

            var enterToSend = PressEnterSendCheckbox.IsChecked ?? false;
            if (!enterToSend)
            {
                return;
            }

            SendMessage();
        }

        #endregion

        private async void StartDiscordConnection()
        {
            try
            {
                await _discordManager.StartClientAsync();
            }
            catch (HttpException ex) when (ex.HttpCode == System.Net.HttpStatusCode.Unauthorized)
            {
                AddMessageHistory("Bad client token. Unauthorized.");
            }
            catch (Exception ex)
            {
                AddMessageHistory($"Error! Exception:{ex}");

            }
        }

        public async void SendMessage()
        {
            var messageToSend = MessageTextBox.Text;
            if (string.IsNullOrWhiteSpace(messageToSend))
            {
                return;
            }

            var isConnected = _discordManager.IsConnected;
            if (!isConnected)
            {
                AddMessageHistory($"Failed to send message, not connected.");
                return;
            }

            var selectedChannel = DiscordConnectionView.SelectedChannel;
            if (selectedChannel == null)
            {
                AddMessageHistory($"Please select a channel before sending.");
                return;
            }

            try
            {
                SendMessageButton.IsEnabled = false;
                await _discordManager.SendMessageAsync(selectedChannel.ChannelId, messageToSend);
                AddMessageHistory(messageToSend);
                MessageTextBox.Text = "";
            } 
            catch (Exception ex)
            {
                AddMessageHistory($"Error: {ex.Message}");
            }
            finally
            {
                SendMessageButton.IsEnabled = true;
            }
        }

        private void AddMessageHistory(string message)
        {
            MessageHistoryTextBlock.Text += $"[{GetTimestamp()}] {message}\n";

            //TODO Fix auto scroll. Apparently ScrollViewer.cs is not the latest version on the release.
            //Update avalonia to a newer version when fixed
            MessageHistoryScrollViewer.Offset = new Vector(double.NegativeInfinity, double.PositiveInfinity);
        }

        private static string GetTimestamp()
        {
            var currentTime = DateTime.Now;
            return currentTime.ToString("HH:mm:ss");
        }
    }
}

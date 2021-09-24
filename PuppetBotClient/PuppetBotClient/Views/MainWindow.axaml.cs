using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Discord;
using Discord.Net;
using PuppetBotClient.Discord;
using PuppetBotClient.Models;
using PuppetBotClient.ViewModels.Discord;
using PuppetBotClient.ViewModels.Emoji;
using PuppetBotClient.Views.EmojiPicker;
using System;
using System.Collections.Generic;

namespace PuppetBotClient.Views
{
    public class MainWindow : Window
    {
        private DiscordConnectionView DiscordConnectionView { get; }
        private ComboBox StatusComboBox { get; }
        private CheckBox PressEnterSendCheckbox { get; }
        private Button SendMessageButton { get; }
        private Button EditMessageButton { get; }
        private Button SetStatusButton { get; }
        private Button EmojisButton { get; }
        private TextBox MessageTextBox { get; }
        private TextBlock MessageHistoryTextBlock { get; }
        private ScrollViewer MessageHistoryScrollViewer { get; }

        private DiscordManager _discordManager;

        public MainWindow()
        {
            InitializeComponent();

            _discordManager = new DiscordManager();

            // UI Controls
            DiscordConnectionView = this.Find<DiscordConnectionView>(nameof(DiscordConnectionView));
            PressEnterSendCheckbox = this.Find<CheckBox>(nameof(PressEnterSendCheckbox));
            SendMessageButton = this.Find<Button>(nameof(SendMessageButton));
            EditMessageButton = this.Find<Button>(nameof(EditMessageButton));
            SetStatusButton = this.Find<Button>(nameof(SetStatusButton));
            EmojisButton = this.Find<Button>(nameof(EmojisButton));
            MessageTextBox = this.Find<TextBox>(nameof(MessageTextBox));
            MessageHistoryTextBlock = this.Find<TextBlock>(nameof(MessageHistoryTextBlock));
            MessageHistoryScrollViewer = this.Find<ScrollViewer>(nameof(MessageHistoryScrollViewer));
            StatusComboBox = this.Find<ComboBox>(nameof(StatusComboBox));

            // Events
            SendMessageButton.Click += SendMessageButton_Clicked;
            EditMessageButton.Click += EditMessageButton_Click;
            SetStatusButton.Click += SetStatusButton_Click;
            EmojisButton.Click += EmojisButton_Click;
            MessageTextBox.KeyUp += MessageTextBox_EnterPressed;
            StatusComboBox.SelectionChanged += StatusComboBox_SelectionChanged;

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
                var serverSelection = await _discordManager.GetServerChannelSelection();
                DiscordConnectionView.SetConnectionStatus(DiscordConnectionView.ConnectionStatus.Connected);
                DiscordConnectionView.SetUserViewModel(currentUser);
                DiscordConnectionView.SetChannelSelectionModel(serverSelection);
                AddMessageHistory($"Connected as {currentUser.Username}");
                EditMessageButton.IsEnabled = true;
                EmojisButton.IsEnabled = true;
                SetStatusButton.IsEnabled = true;
                StatusComboBox.IsEnabled = true;
            });
        }

        private void DiscordManager_Disconnected(Exception ex)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                DiscordConnectionView.SetConnectionStatus(DiscordConnectionView.ConnectionStatus.Disconnected);
                EditMessageButton.IsEnabled = false;
                if (ex != null)
                {
                    AddMessageHistory($"Disconnect Error! ${ex.Message}");
                }
            });
        }

        private void DiscordManager_UserUpdated(DiscordUserViewModel updatedUser)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await DiscordConnectionView.SetUserViewModel(updatedUser);
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

        private void EmojisButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedServerId = DiscordConnectionView.SelectedServer?.ServerId;
            if (!selectedServerId.HasValue)
            {
                return;
            }

            var emojiPicker = new EmojiPickerWindow();
            emojiPicker.EmojiClicked += EmojiPicker_EmojiClicked;
            emojiPicker.LoadEmojisAsync(_discordManager, selectedServerId.Value);
            emojiPicker.Show();
        }

        private void EmojiPicker_EmojiClicked(object sender, EmojiViewModel emoji)
        {
            MessageTextBox.Text += emoji.ToDiscordMessageString();
        }

        private static readonly IReadOnlyDictionary<string, UserStatus> _statusMapping = new Dictionary<string, UserStatus>()
        {
            ["Online"] = UserStatus.Online,
            ["Offline"] = UserStatus.Offline,
            ["AFK"] = UserStatus.AFK,
            ["Idle"] = UserStatus.Idle,
            ["Do Not Disturb"] = UserStatus.DoNotDisturb,
            ["Invisible"] = UserStatus.Invisible,
        };
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedStatus = StatusComboBox.SelectedItem as ComboBoxItem;
            var status = _statusMapping[selectedStatus.Content.ToString()];

            _discordManager.SetStatusAsync(status);
            AddMessageHistory($"[UserStatus] Status set to {status}");
        }

        private async void SetStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedServerId = DiscordConnectionView.SelectedServer?.ServerId;
            if (!selectedServerId.HasValue)
            {
                return;
            }

            var emojiPicker = new SetActivityDialog();
            var dialogResult = await emojiPicker.ShowDialog<SetActivityResultModel>(this);

            if (dialogResult == null)
            {
                return;
            }

            if (dialogResult.ClearActivity)
            {
                // Clear status
                _discordManager.ClearGameActivityAsync();
                AddMessageHistory($"[GameActivity] Game activity cleared");
            }
            else
            {
                // Set status
                _discordManager.SetGameAsync(dialogResult.ActivityName, dialogResult.StreamUrl, dialogResult.ActivityType);
                AddMessageHistory($"[GameActivity] Game activity set to \"{dialogResult.ActivityType} {dialogResult.ActivityName}\"");
            }
        }

        private void EditMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var channelId = DiscordConnectionView.SelectedChannel.ChannelId;
            var editViewModel = new DiscordEditMessageViewModel()
            {
                ChannelId = channelId,
            };

            var editWindow = new EditWindow();
            editWindow.SetDiscordManager(_discordManager);
            editWindow.SetEditViewModel(editViewModel);
            editWindow.Show();
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

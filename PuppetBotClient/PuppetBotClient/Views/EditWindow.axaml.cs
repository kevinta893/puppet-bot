using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PuppetBotClient.Discord;
using PuppetBotClient.ViewModels.Discord;
using System;
using System.Threading.Tasks;

namespace PuppetBotClient.Views
{
    public partial class EditWindow : Window
    {
        private Button GetMessageButton { get; }
        private Button EditMessageButton { get; }
        private TextBox ChannelIdTextBox { get; }
        private TextBox MessageIdTextBox { get; }
        private TextBlock MessageDisplayTextBlock { get; }
        private ScrollViewer MessageDisplayScrollViewer { get; }
        private TextBox MessageTextBox { get; }

        private DiscordManager _discordManager;

        public EditWindow()
        {
            InitializeComponent();

            GetMessageButton = this.Find<Button>(nameof(GetMessageButton));
            EditMessageButton = this.Find<Button>(nameof(EditMessageButton));
            ChannelIdTextBox = this.Find<TextBox>(nameof(ChannelIdTextBox));
            MessageIdTextBox = this.Find<TextBox>(nameof(MessageIdTextBox));
            MessageDisplayTextBlock = this.Find<TextBlock>(nameof(MessageDisplayTextBlock));
            MessageDisplayScrollViewer = this.Find<ScrollViewer>(nameof(MessageDisplayScrollViewer));
            MessageTextBox = this.Find<TextBox>(nameof(MessageTextBox));

            // Events
            GetMessageButton.Click += GetMessageButton_Click;
            EditMessageButton.Click += EditMessageButton_Click;
            MessageTextBox.KeyUp += MessageTextBox_KeyUp;

            // Window setup
            EnableEditMessageControls(false);
        }

        private void MessageTextBox_KeyUp(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                EditMessageButton.IsEnabled = false;
            }
            else
            {
                EditMessageButton.IsEnabled = true;
            }
        }

        public void SetDiscordManager(DiscordManager discordManager)
        {
            _discordManager = discordManager;
        }

        public void SetEditViewModel(DiscordEditMessageViewModel viewModel)
        {
            ChannelIdTextBox.Text = viewModel.ChannelId.HasValue ? viewModel.ChannelId.ToString() : "";
        }

        private void EditMessageButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(async () => {
                try
                {
                    ulong.TryParse(ChannelIdTextBox.Text, out ulong channelId);
                    ulong.TryParse(MessageIdTextBox.Text, out ulong messageId);
                    var editedMessage = MessageTextBox.Text;
                    await _discordManager.EditMessageAsync(channelId, messageId, editedMessage);
                }
                catch (Exception ex)
                {
                    MessageDisplayTextBlock.Text = $"Error: {ex.Message}";
                }
            });
        }

        private void GetMessageButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            EnableEditMessageControls(false);
            Dispatcher.UIThread.InvokeAsync(async () => {
                try
                {
                    ulong.TryParse(ChannelIdTextBox.Text, out ulong channelId);
                    ulong.TryParse(MessageIdTextBox.Text, out ulong messageId);
                    var message = await _discordManager.GetMessageAsync(channelId, messageId);
                    if (message == null)
                    {
                        MessageDisplayTextBlock.Text = $"Error: Invalid message Id or channel Id";
                        return;
                    }

                    MessageDisplayTextBlock.Text = message.Content;
                    MessageTextBox.Text = message.Content;
                    EnableEditMessageControls(true);
                }
                catch (Exception ex)
                {
                    EnableEditMessageControls(false);
                    MessageDisplayTextBlock.Text = $"Error: {ex.Message}";
                }
            });
        }

        private void EnableEditMessageControls(bool enabled)
        {
            MessageTextBox.IsEnabled = enabled;
            EditMessageButton.IsEnabled = enabled;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

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

            // Window setup
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

        }

        private void GetMessageButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                ulong.TryParse(ChannelIdTextBox.Text, out ulong channelId);
                ulong.TryParse(MessageIdTextBox.Text, out ulong messageId);
                var message = _discordManager.GetMessageAsync(channelId, messageId).Result;
                MessageDisplayTextBlock.Text = message.Content;
                MessageTextBox.Text = message.Content;
            }
            catch (Exception ex)
            {
                MessageDisplayTextBlock.Text = $"Error: {ex.Message}";
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

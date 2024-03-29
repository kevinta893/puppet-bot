using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PuppetBotClient.Discord;
using PuppetBotClient.ViewModels.Emoji;
using System;
using System.Threading.Tasks;

namespace PuppetBotClient.Views.EmojiPicker
{
    public partial class EmojiPickerWindow : Window
    {
        private StackPanel ServerEmojisList;

        public event EventHandler<EmojiViewModel> EmojiClicked;

        public EmojiPickerWindow()
        {
            InitializeComponent();

            ServerEmojisList = this.Find<StackPanel>(nameof(ServerEmojisList));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void SetNamedTitle(string name)
        {
            this.Title = $"{name}: {Title}";
        }

        public async Task LoadEmojisAsync(DiscordManager discord, ulong guildId)
        {
            var serverEmojis = new[] { await discord.GetServerEmoji(guildId) };

            foreach (var serverEmoji in serverEmojis)
            {
                var serverEmojiCollection = new ServerEmojiCollectionView();
                serverEmojiCollection.EmojiClicked += ServerEmojiCollection_EmojiClicked;
                serverEmojiCollection.LoadEmojiCollection(serverEmoji);
                serverEmojiCollection.Margin = new Thickness(0, 0, 0, 10);

                ServerEmojisList.Children.Add(serverEmojiCollection);
            }
        }

        private void ServerEmojiCollection_EmojiClicked(object sender, EmojiViewModel emoji)
        {
            EmojiClicked.Invoke(sender, emoji);
        }
    }
}

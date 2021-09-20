using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Discord.WebSocket;
using PuppetBotClient.Discord;
using PuppetBotClient.ViewModels.Emoji;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PuppetBotClient.Views
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

        public async Task LoadEmojisAsync(DiscordManager discord)
        {
            var serverEmojis = await discord.GetServerEmojis();

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

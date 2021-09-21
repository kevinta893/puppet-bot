using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PuppetBotClient.ViewModels.Emoji;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PuppetBotClient.Views
{
    public partial class ServerEmojiCollectionView : UserControl
    {
        private readonly Label ServerNameLabel;
        private readonly StackPanel EmojiCollectionCanvas;

        private const int EmojiWidth = 30;
        private const int EmojiHeight = 30;

        private readonly int MaxEmojiInHorizontalStack;

        public event EventHandler<EmojiViewModel> EmojiClicked;

        public ServerEmojiCollectionView()
        {
            InitializeComponent();

            ServerNameLabel = this.Find<Label>(nameof(ServerNameLabel));
            EmojiCollectionCanvas = this.Find<StackPanel>(nameof(EmojiCollectionCanvas));

            MaxEmojiInHorizontalStack = (int) this.Width / EmojiWidth;
        }

        public async Task LoadEmojiCollection(ServerEmojisViewModel viewModel)
        {
            ServerNameLabel.Content = viewModel.Name;

            var emojiIndex = 0;
            var currentRow = 0;
            var rowCount = 0;
            StackPanel currentStackRow = null;
            foreach(var emoji in viewModel.Emojis)
            {
                // Start new row
                if (emojiIndex == currentRow)
                {
                    rowCount++;
                    currentRow += MaxEmojiInHorizontalStack;
                    currentStackRow = new StackPanel();
                    currentStackRow.Orientation = Avalonia.Layout.Orientation.Horizontal;
                    EmojiCollectionCanvas.Children.Add(currentStackRow);
                }

                var emojiView = new EmojiView();
                emojiView.LoadEmojiViewModel(emoji);
                emojiView.SetHeight(EmojiHeight);
                emojiView.SetWidth(EmojiWidth);
                emojiView.Click += EmojiView_Click;

                currentStackRow.Children.Add(emojiView);

                emojiIndex++;
            }

            this.Height = ((rowCount) * EmojiHeight) + ServerNameLabel.Height + 10;
        }

        private void EmojiView_Click(object sender, EmojiViewModel emoji)
        {
            EmojiClicked.Invoke(sender, emoji);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

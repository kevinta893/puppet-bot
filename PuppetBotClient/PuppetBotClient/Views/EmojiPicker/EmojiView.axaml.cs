using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PuppetBotClient.ImageCache;
using PuppetBotClient.ViewModels.Emoji;
using System;
using System.Threading.Tasks;

namespace PuppetBotClient.Views.EmojiPicker
{
    public partial class EmojiView : UserControl
    {
        private readonly Image EmojiPreview;
        private readonly Rectangle EmojiBackground;
        private readonly Image GifIndicator;

        private EmojiViewModel _emojiViewModel;

        private static readonly ImageUrlCacher _imgCache = new ImageUrlCacher();

        private const float GifIndicatorScale = 0.5f;      //Percent scale of the emoji preview

        public event EventHandler<EmojiViewModel> Click;

        public EmojiView()
        {
            InitializeComponent();

            EmojiPreview = this.Find<Image>(nameof(EmojiPreview));
            EmojiBackground = this.Find<Rectangle>(nameof(EmojiBackground));
            GifIndicator = this.Find<Image>(nameof(GifIndicator));

            EmojiBackground.PointerReleased += EmojiPreview_PointerReleased;
            EmojiPreview.PointerReleased += EmojiPreview_PointerReleased;
        }

        public void SetHeight(int height)
        {
            this.Height = height;
            EmojiPreview.Height = height;
            EmojiBackground.Height = height;
            GifIndicator.Height = height * GifIndicatorScale;
        }

        public void SetWidth (int width)
        {
            this.Width = width;
            EmojiPreview.Width = width;
            EmojiBackground.Width = width;
            GifIndicator.Width = width * GifIndicatorScale;
        }

        public async Task LoadEmojiViewModel(EmojiViewModel viewModel)
        {
            _emojiViewModel = viewModel;
            GifIndicator.IsVisible = viewModel.IsAnimated;

            var animatedTag = viewModel.IsAnimated ? "[animated]" : "";
            ToolTip.SetTip(EmojiPreview, $"{viewModel.Alias} {animatedTag}");
            ToolTip.SetTip(GifIndicator, $"{viewModel.Alias} {animatedTag}");

            var imageUrl = viewModel.ImageUrl;
            var cachedImageStream = _imgCache.GetCachedImage(imageUrl);
            var imageSource = new Bitmap(cachedImageStream);
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                EmojiPreview.Source = imageSource;
            }, DispatcherPriority.Background);
        }

        private void EmojiPreview_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            Click.Invoke(sender, _emojiViewModel);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PuppetBotClient.ImageCache;
using PuppetBotClient.Util;
using PuppetBotClient.ViewModels.Emoji;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PuppetBotClient.Views
{
    public partial class EmojiView : UserControl
    {
        private readonly Image EmojiPreview;
        private readonly Rectangle EmojiBackground;
        private EmojiViewModel _emojiViewModel;

        private static readonly ImageUrlCacher _imgCache = new ImageUrlCacher();

        public event EventHandler<EmojiViewModel> Click;

        public EmojiView()
        {
            InitializeComponent();

            EmojiPreview = this.Find<Image>(nameof(EmojiPreview));
            EmojiBackground = this.Find<Rectangle>(nameof(EmojiBackground));

            EmojiBackground.PointerReleased += EmojiPreview_PointerReleased;
            EmojiPreview.PointerReleased += EmojiPreview_PointerReleased;
        }

        public void SetHeight(int height)
        {
            this.Height = height;
            EmojiPreview.Height = height;
            EmojiBackground.Height = height;
        }

        public void SetWidth (int width)
        {
            this.Width = width;
            EmojiPreview.Width = width;
            EmojiBackground.Width = width;
        }

        public async Task LoadEmojiViewModel(EmojiViewModel viewModel)
        {
            _emojiViewModel = viewModel;

            //Task.Run(async () =>
            //{
                var imageUrl = viewModel.ImageUrl;
                var imageName = viewModel.Alias + ".png";

                var cachedImageStream = _imgCache.GetCachedImage(imageUrl);
                if (cachedImageStream == null)
                {
                    cachedImageStream = await UrlImageHelper.GetImageStreamFromUrl(imageUrl);
                    _imgCache.StoreImageToCache(imageUrl, imageName, cachedImageStream);
                }

                cachedImageStream.Seek(0, SeekOrigin.Begin);
                var imageSource = new Bitmap(cachedImageStream);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    EmojiPreview.Source = imageSource;
                }, DispatcherPriority.Background);
            //});
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

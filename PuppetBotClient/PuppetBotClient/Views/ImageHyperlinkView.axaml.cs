using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Diagnostics;

namespace PuppetBotClient.Views
{
    public class ImageHyperlinkView : UserControl
    {
        private readonly IBrush NoHoverFillColor = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        private readonly IBrush HoverFillColor = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0));
        private readonly Cursor Arrow = new Cursor(StandardCursorType.Arrow);
        private readonly Cursor Hand = new Cursor(StandardCursorType.Hand);

        private Rectangle UserAvatarBackground { get; }
        private Image UserAvatarImage { get; }
        private TextBlock HyperlinkToolTipText { get; }
        private Rectangle HoverTextBackground { get; }

        private bool _isHover;

        public string HyperLink { get; set; }


        public ImageHyperlinkView()
        {
            this.InitializeComponent();

            UserAvatarBackground = this.Find<Rectangle>(nameof(UserAvatarBackground));
            UserAvatarImage = this.Find<Image>(nameof(UserAvatarImage));
            HyperlinkToolTipText = this.Find<TextBlock>(nameof(HyperlinkToolTipText));
            HoverTextBackground = this.Find<Rectangle>(nameof(HoverTextBackground));

            HoverTextBackground.PointerEnter += HoverTextBackground_PointerEnter;
            HoverTextBackground.PointerLeave += HoverTextBackground_PointerLeave;
            HoverTextBackground.PointerPressed += HoverTextBackground_PointerPressed;
            HoverTextBackground.PointerMoved += HoverTextBackground_PointerMoved;
            HyperlinkToolTipText.PointerEnter += HoverTextBackground_PointerEnter;
            HyperlinkToolTipText.PointerLeave += HoverTextBackground_PointerLeave;
            HyperlinkToolTipText.PointerMoved += HoverTextBackground_PointerMoved;
            HyperlinkToolTipText.PointerPressed += HoverTextBackground_PointerPressed;
        }

        public IBitmap ImageSource
        {
            get
            {
                return UserAvatarImage.Source;
            }
            set
            {
                UserAvatarImage.Source = value;
            }
        }

        public string HyperLinkToolTip
        {
            get
            {
                return HyperlinkToolTipText.Text;
            }
            set
            {
                HyperlinkToolTipText.Text = value;
            }
        }

        private void HoverTextBackground_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(HyperLink)) 
            { 
                return; 
            }

            var startInfo = new ProcessStartInfo()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = "cmd.exe",
                Arguments = $"/c start {HyperLink}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            Process.Start(startInfo);
        }

        private void HoverTextBackground_PointerMoved(object sender, PointerEventArgs e)
        {
            IsHover = true;
        }

        private void HoverTextBackground_PointerLeave(object sender, Avalonia.Input.PointerEventArgs e)
        {
            IsHover = false;
        }

        private void HoverTextBackground_PointerEnter(object sender, Avalonia.Input.PointerEventArgs e)
        {
            IsHover = true;
        }

        private bool IsHover { 
            get 
            {
                return _isHover;
            } 
            set 
            {
                _isHover = value;
                if (string.IsNullOrWhiteSpace(HyperLink))
                {
                    // No link
                    HyperlinkToolTipText.Cursor = Arrow;
                    HoverTextBackground.Cursor = Arrow;
                    ShowHoverText(false);
                    return;
                }

                HyperlinkToolTipText.Cursor = Hand;
                HoverTextBackground.Cursor = Hand;
                ShowHoverText(_isHover);
            } 
        }

        /// <summary>
        /// Displays 
        /// </summary>
        /// <param name="showHover"></param>
        private void ShowHoverText(bool showHover)
        {
            if (showHover)
            {
                // Hover over
                HyperlinkToolTipText.IsVisible = true;
                HoverTextBackground.Fill = HoverFillColor;
            }
            else
            {
                // No hover
                HyperlinkToolTipText.IsVisible = false;
                HoverTextBackground.Fill = NoHoverFillColor;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

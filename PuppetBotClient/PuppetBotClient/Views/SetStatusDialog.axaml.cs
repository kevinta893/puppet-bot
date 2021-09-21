using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PuppetBotClient.Views
{
    public partial class SetStatusDialog : Window
    {
        public SetStatusDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

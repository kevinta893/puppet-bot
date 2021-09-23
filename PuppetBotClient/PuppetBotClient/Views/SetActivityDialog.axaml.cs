using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Discord;
using PuppetBotClient.Models;
using System.Collections.Generic;

namespace PuppetBotClient.Views
{
    public partial class SetActivityDialog : Window
    {
        private readonly TextBox StatusTextBox;
        private readonly TextBox StreamUrlTextBox;
        private readonly ComboBox ActivityComboBox;
        private readonly Button OkButton;
        private readonly Button CancelButton;

        private readonly IReadOnlyDictionary<string, ActivityType> _activityMapping = new Dictionary<string, ActivityType>()
        {
            ["Playing"] = ActivityType.Playing,
            ["Streaming"] = ActivityType.Streaming,
            ["Watching"] = ActivityType.Watching,
            ["Listening"] = ActivityType.Listening,
        };

        public SetActivityDialog()
        {
            InitializeComponent();

            StatusTextBox = this.Find<TextBox>(nameof(StatusTextBox));
            OkButton = this.Find<Button>(nameof(OkButton));
            CancelButton = this.Find<Button>(nameof(CancelButton));
            StreamUrlTextBox = this.Find<TextBox>(nameof(StreamUrlTextBox));
            ActivityComboBox = this.Find<ComboBox>(nameof(ActivityComboBox));

            OkButton.Click += OkButton_Click;
            CancelButton.Click += CancelButton_Click;
            ActivityComboBox.SelectionChanged += ActivityComboBox_SelectionChanged;
        }

        private void ActivityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedActivity = ActivityComboBox.SelectedItem as ComboBoxItem;
            var activityType = _activityMapping[selectedActivity.Content?.ToString()];

            if (activityType == ActivityType.Streaming)
            {
                StreamUrlTextBox.Text = "";
                StreamUrlTextBox.IsEnabled = true;
            }
            else
            {
                StreamUrlTextBox.Text = "";
                StreamUrlTextBox.IsEnabled = false;
            }
        }

        private void CancelButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var statusText = StatusTextBox.Text;

            var selectedActivity = ActivityComboBox.SelectedItem as ComboBoxItem;
            var activityType = _activityMapping[selectedActivity.Content?.ToString()];
            var setActivityResult = new SetActivityResultModel()
            {
                ClearActivity = string.IsNullOrEmpty(statusText),
                ActivityName = statusText,                              //Note max length is 128
                StreamUrl = StreamUrlTextBox.Text,
                ActivityType = activityType,
            };
            Close(setActivityResult);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

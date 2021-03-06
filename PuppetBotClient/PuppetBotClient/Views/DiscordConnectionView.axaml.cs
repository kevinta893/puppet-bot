﻿using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using PuppetBotClient.ViewModels.Discord;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;

namespace PuppetBotClient.Views
{
    public delegate void DiscordUserUpdatedEvent(DiscordUserViewModel updatedUser);

    public class DiscordConnectionView : UserControl
    {
        private TextBlock UsernameText { get; }
        private ImageHyperlinkView UserAvatarImage { get; }
        private Image DiscordConnectedIndicator { get; }
        private Image DiscordConnectingIndicator { get; }
        private Image DiscordDisconnectedIndicator { get; }
        private ComboBox SelectServerComboBox { get; }
        private ComboBox SelectChannelComboBox { get; }

        private IDictionary<ConnectionStatus, Image> _connectionStatusIcons;

        public DiscordServerViewModel SelectedServer 
        { 
            get
            {
                return SelectServerComboBox.SelectedItem as DiscordServerViewModel;
            }
        }

        public DiscordChannelViewModel SelectedChannel 
        {
            get
            {
                return SelectChannelComboBox.SelectedItem as DiscordChannelViewModel;
            }
        }
        public DiscordConnectionView()
        {
            this.InitializeComponent();

            UsernameText = this.Find<TextBlock>(nameof(UsernameText));
            UserAvatarImage = this.Find<ImageHyperlinkView>(nameof(UserAvatarImage));
            DiscordConnectedIndicator = this.Find<Image>(nameof(DiscordConnectedIndicator));
            DiscordConnectingIndicator = this.Find<Image>(nameof(DiscordConnectingIndicator));
            DiscordDisconnectedIndicator = this.Find<Image>(nameof(DiscordDisconnectedIndicator));
            SelectServerComboBox = this.Find<ComboBox>(nameof(SelectServerComboBox));
            SelectChannelComboBox = this.Find<ComboBox>(nameof(SelectChannelComboBox));

            _connectionStatusIcons = new Dictionary<ConnectionStatus, Image>()
            {
                { ConnectionStatus.Connected , DiscordConnectedIndicator},
                { ConnectionStatus.Connecting , DiscordConnectingIndicator},
                { ConnectionStatus.Disconnected , DiscordDisconnectedIndicator},
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void SetConnectionStatus(ConnectionStatus connectionStatus)
        {
            var otherStatuses = _connectionStatusIcons.Keys.Where(status => status != connectionStatus);
            foreach (var status in otherStatuses)
            {
                _connectionStatusIcons[status].IsVisible = false;
            }
            _connectionStatusIcons[connectionStatus].IsVisible = true;
        }

        public void SetUserViewModel(DiscordUserViewModel userModel)
        {
            UsernameText.Text = userModel.Username;
            UserAvatarImage.ImageSource = GetImageFromUrl(userModel.AvatarImageUrl);
            UserAvatarImage.HyperLink = userModel.BotManagementUrl;
            UserAvatarImage.HyperLinkToolTip = "Open Bot\nSettings\n(web)";
        }

        public void SetChannelSelectionModel(DiscordChannelSelectionViewModel discordChannelSelectionModel)
        {
            SelectServerComboBox.Items = discordChannelSelectionModel.Servers
                .Select(server => server.Value);

            SelectServerComboBox.SelectionChanged += (sender, e) =>
            {
                SelectChannelComboBox.Items = Enumerable.Empty<DiscordChannelSelectionViewModel>();
                var selectedServer = SelectServerComboBox.SelectedItem as DiscordServerViewModel;
                SelectChannelComboBox.Items = selectedServer.Channels;
            };

            SetControlsIsEnabled(true);
            SelectServerComboBox.SelectedIndex = 0;
            SelectChannelComboBox.SelectedIndex = 0;
        }

        public void SetControlsIsEnabled(bool isEnabled)
        {
            SelectServerComboBox.IsEnabled = isEnabled;
            SelectChannelComboBox.IsEnabled = isEnabled;
        }

        private static IBitmap GetImageFromUrl(string imageUrl)
        {
            using (var webClient = new WebClient())
            {
                var stream = webClient.OpenRead(imageUrl);
                var bitmap = new System.Drawing.Bitmap(stream);
                var tempStream = new MemoryStream();
                bitmap.Save(tempStream, ImageFormat.Bmp);
                tempStream.Seek(0, SeekOrigin.Begin);

                var uiBitmap = new Bitmap(tempStream);

                stream.Flush();
                stream.Close();

                return uiBitmap;
            }
        }

        public enum ConnectionStatus
        {
            Connected,
            Connecting,
            Disconnected,
        }
    }
}

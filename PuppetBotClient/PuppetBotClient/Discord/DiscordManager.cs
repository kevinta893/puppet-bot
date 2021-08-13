using Discord;
using Discord.WebSocket;
using PuppetBotClient.Util;
using PuppetBotClient.ViewModels.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PuppetBotClient.Discord
{
    public delegate void DiscordConnectionEvent();
    public delegate void DiscordDisconnectionEvent(Exception ex);
    public delegate void DiscordUserUpdatedEvent(DiscordUserViewModel updatedUser);

    public class DiscordManager
    {
        private readonly DiscordSocketClient _discordClient;

        public event DiscordDisconnectionEvent Disconnected;
        public event DiscordConnectionEvent Connected;
        public event DiscordUserUpdatedEvent UserUpdated;

        public bool IsConnected => _discordClient.LoginState == LoginState.LoggedIn;

        public DiscordManager()
        {
            //_configuration = configuration;
            _discordClient = new DiscordSocketClient();

            // Discord Events
            _discordClient.Disconnected += DiscordClient_Disconnected;
            _discordClient.Ready += DiscordClient_Connected;
            _discordClient.CurrentUserUpdated += DiscordClient_CurrentUserUpdated;
        }

        #region Discord Events

        private Task DiscordClient_CurrentUserUpdated(SocketSelfUser arg1, SocketSelfUser arg2)
        {
            var updatedUser = CreateUserViewModel(arg2);
            UserUpdated?.Invoke(updatedUser);
            return Task.CompletedTask;
        }

        private Task DiscordClient_Connected()
        {
            Connected?.Invoke();
            return Task.CompletedTask;
        }

        private Task DiscordClient_Disconnected(Exception ex)
        {
            Disconnected?.Invoke(ex);
            return Task.CompletedTask;
        }

        #endregion

        public async Task StartClientAsync()
        {
            if (_discordClient.LoginState != LoginState.LoggedOut)
            {
                return;
            }

            // Get command line bot token index if any
            var commandArgs = Environment.GetCommandLineArgs();
            var botTokenIndex = 0;
            var hasBotIndexArg = commandArgs.Length > 1 && int.TryParse(commandArgs[1], out botTokenIndex);
            if (!hasBotIndexArg)
            {
                botTokenIndex = 0;
            }

            // Start client
            var botToken = AppConfiguration.Settings.Discord.BotTokens[botTokenIndex];
            await _discordClient.LoginAsync(TokenType.Bot, botToken);
            await _discordClient.StartAsync();
        }

        public async Task DisconnectClient()
        {
            await _discordClient.LogoutAsync();
        }

        public async Task<DiscordUserViewModel> GetCurrentDiscordUser()
        {
            var currentUser = _discordClient.CurrentUser;
            return CreateUserViewModel(currentUser);
        }

        private DiscordUserViewModel CreateUserViewModel(SocketSelfUser socketSelfUser)
        {
            var discordUser = new DiscordUserViewModel()
            {
                Username = $"{socketSelfUser.Username}#{socketSelfUser.Discriminator}",
                AvatarImageUrl = socketSelfUser.GetAvatarUrl(),
                BotManagementUrl = $"https://discord.com/developers/applications/{socketSelfUser.Id}/bot",
            };

            return discordUser;
        }

        public async Task<DiscordChannelSelectionViewModel> GetServerSelection()
        {
            var invitedServers = await GetInvitedServersAsync();
            var allChannels = new List<ITextChannel>();
            foreach (var server in invitedServers)
            {
                var channels = await GetAllTextChannelsAsync(server);
                allChannels.AddRange(channels);
            }
            //await Task.WhenAll(allChannels);

            var channelDictionary = allChannels
                //.SelectMany(channels => channels)
                .GroupBy(channel => channel.GuildId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var serverSelection = new DiscordChannelSelectionViewModel
            {
                Servers = invitedServers.Select(server => new DiscordServerViewModel
                {
                    Name = server.Name,
                    ServerId = server.Id,
                    Channels = channelDictionary[server.Id].Select(channel => new DiscordChannelViewModel
                    {
                        Name = channel.Name,
                        ChannelId = channel.Id,
                    }),
                })
                .ToDictionary(serverModel => serverModel.ServerId, serverModel => serverModel)
            };

            return serverSelection;
        }

        public async Task<IEnumerable<IGuild>> GetInvitedServersAsync()
        {
            var guilds = await (_discordClient as IDiscordClient).GetGuildsAsync();
            return guilds;
        }

        public async Task<IEnumerable<ITextChannel>> GetAllTextChannelsAsync(IGuild guild)
        {
            var guildTextChannels = await guild.GetTextChannelsAsync();
            return guildTextChannels;
        }

        public async Task SendMessageAsync(ulong channelId, string message)
        {
            var channel = _discordClient.GetChannel(channelId) as IMessageChannel;
            await channel.SendMessageAsync(message);
        }

        public async Task<IMessage> GetMessageAsync(ulong channelId, ulong messageId)
        {
            var channel = _discordClient.GetChannel(channelId) as IMessageChannel;
            var message = await channel.GetMessageAsync(messageId);
            return message;
        }

        public async Task EditMessageAsync(ulong channelId, ulong messageId, string editedMessage)
        {
            var channel = _discordClient.GetChannel(channelId) as IMessageChannel;
            var message = await channel.GetMessageAsync(messageId) as IUserMessage;
            await message.ModifyAsync(msg => msg.Content = editedMessage);
        }
    }
}

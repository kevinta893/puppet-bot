using System.Collections.Generic;

namespace PuppetBotClient.ViewModels.Discord
{
    public class DiscordChannelSelectionViewModel
    {
        public IDictionary<ulong, DiscordServerViewModel> Servers { get; set; }
    }
}

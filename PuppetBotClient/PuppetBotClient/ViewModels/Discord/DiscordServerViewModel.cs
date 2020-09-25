using System.Collections.Generic;

namespace PuppetBotClient.ViewModels.Discord
{
    public class DiscordServerViewModel
    {
        public string Name { get; set; }
        public ulong ServerId { get; set; }
        public IEnumerable<DiscordChannelViewModel> Channels { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

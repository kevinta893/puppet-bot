using System.Collections.Generic;

namespace PuppetBotClient.ViewModels.Emoji
{
    public class ServerEmojisViewModel
    {
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public IEnumerable<EmojiViewModel> Emojis { get; set; }
    }
}

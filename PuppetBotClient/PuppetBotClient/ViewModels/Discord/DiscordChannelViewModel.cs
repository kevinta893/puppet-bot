namespace PuppetBotClient.ViewModels.Discord
{
    public class DiscordChannelViewModel
    {
        public string Name { get; set; }
        public ulong ChannelId { get; set; }
        public int SortNo { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

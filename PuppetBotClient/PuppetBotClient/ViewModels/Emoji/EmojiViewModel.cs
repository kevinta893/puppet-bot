namespace PuppetBotClient.ViewModels.Emoji
{
    public class EmojiViewModel
    {
        public ulong EmojiId { get; set; }
        public string Alias { get; set; }
        public bool IsAnimated { get; set; }
        public string ImageUrl { get; set; }
        
        public string ToDiscordMessageString()
        {
            var animatedFlag = IsAnimated ? "a" : "";
            return $"<{animatedFlag}:{Alias}:{EmojiId}>";
        }
    }
}

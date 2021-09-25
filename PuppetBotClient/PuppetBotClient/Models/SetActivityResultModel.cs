using Discord;

namespace PuppetBotClient.Models
{
    public class SetActivityResultModel
    {
        public bool ClearActivity { get; set; }
        public string ActivityName { get; set; }
        public ActivityType ActivityType { get; set; }
        public string StreamUrl { get; set; }
    }
}

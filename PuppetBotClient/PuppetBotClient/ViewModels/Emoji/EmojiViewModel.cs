using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace PuppetBotClient.ViewModels.Emoji
{
    public class EmojiViewModel
    {
        public ulong EmojiId { get; set; }
        public string Alias { get; set; }
        public bool Animated { get; set; }
        public string ImageUrl { get; set; }
        
        public string ToDiscordMessageString()
        {
            return $"<:{Alias}:{EmojiId}>";
        }
    }
}

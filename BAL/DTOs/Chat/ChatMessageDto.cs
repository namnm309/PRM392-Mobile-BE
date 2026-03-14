using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Chat
{
    /// <summary>
    /// Message trong cuộc hội thoại chat
    /// </summary>
    public class ChatMessageDto
    {
        /// <summary>system | user | assistant</summary>
        [Required]
        public string Role { get; set; } = "user";

        [Required]
        public string Content { get; set; } = string.Empty;
    }
}

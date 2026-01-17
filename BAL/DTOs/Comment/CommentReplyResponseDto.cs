namespace BAL.DTOs.Comment
{
    /// <summary>
    /// Comment reply response DTO
    /// </summary>
    public class CommentReplyResponseDto
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string ReplyContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

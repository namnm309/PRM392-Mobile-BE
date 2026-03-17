namespace BAL.DTOs.Review
{
    public class ReviewReplyResponseDto
    {
        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string ReplyContent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

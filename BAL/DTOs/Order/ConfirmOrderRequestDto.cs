namespace BAL.DTOs.Order
{
    /// <summary>
    /// Request DTO for confirming order by admin/staff
    /// </summary>
    public class ConfirmOrderRequestDto
    {
        /// <summary>
        /// Optional notes from admin
        /// </summary>
        public string? Notes { get; set; }
    }
}

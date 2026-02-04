using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Category
{
    /// <summary>
    /// DTO cho mỗi category trong bulk create - theo cấu trúc Swagger
    /// </summary>
    public class BulkCreateCategoryItemDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Danh sách tên các category con (sẽ được tạo và gán vào category cha này)
        /// </summary>
        public List<string> Children { get; set; } = new();
    }
}

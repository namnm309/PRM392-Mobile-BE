using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BAL.Converters;

namespace BAL.DTOs.Category
{
    /// <summary>
    /// Create category request DTO
    /// </summary>
    public class CreateCategoryRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsHot { get; set; } = false;

        /// <summary>
        /// Danh sách category con (sẽ được tạo và gán vào category cha này).
        /// Hỗ trợ format: ["Child1", "Child2"] hoặc [{"name":"Child1","imageUrl":null,"displayOrder":0,"isHot":false}]
        /// </summary>
        [JsonConverter(typeof(CategoryChildListJsonConverter))]
        public List<CreateCategoryChildDto> Children { get; set; } = new();
    }
}

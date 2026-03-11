using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    [Table("tbl_linked_accounts")]
    public class LinkedAccount
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("provider")]
        [MaxLength(50)]
        public string Provider { get; set; } = string.Empty;

        [Required]
        [Column("provider_user_id")]
        [MaxLength(255)]
        public string ProviderUserId { get; set; } = string.Empty;

        [Column("provider_email")]
        [MaxLength(255)]
        public string? ProviderEmail { get; set; }

        [Column("provider_name")]
        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [Column("provider_avatar_url")]
        [MaxLength(500)]
        public string? ProviderAvatarUrl { get; set; }

        [Column("linked_at")]
        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

        [Column("last_used_at")]
        public DateTime? LastUsedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    // Entity đại diện cho User trong hệ thống
    // Sync với Clerk authentication service
    
    [Table("tbl_users")]
    [Index(nameof(ClerkId), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(PhoneNumber))]
    public class User
    {
    
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
                
        // Clerk User ID - Dùng để sync với Clerk
        [Required]
        [Column("clerk_id")]
        [MaxLength(100)]
        public string ClerkId { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Column("phone_number")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Column("full_name")]
        [MaxLength(200)]
        public string? FullName { get; set; }

        [Column("avatar_url")]
        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        /// Giới tính: Male, Female, Other       
        [Column("gender")]
        [MaxLength(20)]
        public string? Gender { get; set; }

        [Column("default_address")]
        [MaxLength(500)]
        public string? DefaultAddress { get; set; }

        [Column("city")]
        [MaxLength(100)]
        public string? City { get; set; }
                
        // Điểm tích lũy (loyalty points)       
        [Column("loyalty_points")]
        public int LoyaltyPoints { get; set; } = 0;
                
        // Trạng thái tài khoản: Active, Inactive, Banned        
        [Required]
        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        // Role: Guest, Customer, Staff, Admin       
        [Required]
        [Column("role")]
        [MaxLength(20)]
        public string Role { get; set; } = "Guest";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        // Metadata bổ sung dạng JSON (preferences, settings, etc.)        
        [Column("metadata", TypeName = "jsonb")]
        public string? Metadata { get; set; }

        // Navigation Properties (nếu có relationships)
        // public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        // public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        // public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
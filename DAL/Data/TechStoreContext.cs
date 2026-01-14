using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Data
{
    public class TechStoreContext : DbContext
    {
        // Constructor cho DI (Dependency Injection)
        public TechStoreContext(DbContextOptions<TechStoreContext> options) : base(options)
        {
        }

        // Constructor mặc định cho testing/migration
        public TechStoreContext()
        {
        }

        //========================================================================================================================
        //Khai báo entity 
        //Dbset biểu diễn 1 bảng của csdl 
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentReply> CommentReplies { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherUsage> VoucherUsages { get; set; }
        public DbSet<SupportChatSession> SupportChatSessions { get; set; }
        public DbSet<SupportChatMessage> SupportChatMessages { get; set; }



        //Nếu muốn cấu hình chi tiết thêm thì overrive OnModelCreating
        //Nếu đã sử dụng [] trc các attribute thì có thể ko cần method này 
        //Nếu có 1 số cái phức tạp mà [] ko thể triển khai hết thì nên dùng method này 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Setup lại các thuộc tính cho entity user , đã có [] vẫn có thể settup đc nó sẽ override
            modelBuilder.Entity<User>(entity =>
            {
     
            });

            // Address configuration
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasIndex(a => a.UserId);
                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CartItem configuration
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasIndex(c => new { c.UserId, c.ProductId });
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.Product)
                      .WithMany(p => p.CartItems)
                      .HasForeignKey(c => c.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => o.UserId);
                entity.HasIndex(o => o.CreatedAt);
                entity.HasOne(o => o.User)
                      .WithMany()
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(o => o.Address)
                      .WithMany()
                      .HasForeignKey(o => o.AddressId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasIndex(oi => oi.OrderId);
                entity.HasIndex(oi => new { oi.ProductId, oi.Status });
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(oi => oi.Product)
                      .WithMany()
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Comment configuration - Unique constraint: (userId, productId)
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasIndex(c => c.ProductId);
                entity.HasIndex(c => new { c.UserId, c.ProductId }).IsUnique();
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(c => c.Product)
                      .WithMany()
                      .HasForeignKey(c => c.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CommentReply configuration - Unique constraint: commentId (one reply per comment)
            modelBuilder.Entity<CommentReply>(entity =>
            {
                entity.HasIndex(cr => cr.CommentId).IsUnique();
                entity.HasOne(cr => cr.Comment)
                      .WithOne(c => c.Reply)
                      .HasForeignKey<CommentReply>(cr => cr.CommentId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(cr => cr.Staff)
                      .WithMany()
                      .HasForeignKey(cr => cr.StaffId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Voucher configuration
            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.HasIndex(v => v.Code).IsUnique();
                entity.HasIndex(v => new { v.IsActive, v.StartTime, v.EndTime });
            });

            // VoucherUsage configuration
            modelBuilder.Entity<VoucherUsage>(entity =>
            {
                entity.HasIndex(vu => new { vu.VoucherId, vu.UserId });
                entity.HasIndex(vu => vu.OrderId);
                entity.HasOne(vu => vu.Voucher)
                      .WithMany(v => v.VoucherUsages)
                      .HasForeignKey(vu => vu.VoucherId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name);
                entity.HasIndex(c => c.IsActive);
            });

            // Brand configuration
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasIndex(b => b.Name);
                entity.HasIndex(b => b.IsActive);
            });

            // Product configuration - Update with Category and Brand relationships
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.CategoryId);
                entity.HasIndex(p => p.BrandId);
                entity.HasIndex(p => new { p.IsActive, p.CategoryId, p.BrandId });
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ProductImage configuration
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasIndex(pi => pi.ProductId);
                entity.HasIndex(pi => new { pi.ProductId, pi.ImageType });
                entity.HasIndex(pi => new { pi.ProductId, pi.DisplayOrder });
                entity.HasOne(pi => pi.Product)
                      .WithMany(p => p.ProductImages)
                      .HasForeignKey(pi => pi.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}


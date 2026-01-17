using BAL.Services;
using DAL.Data;
using DAL.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using TechStoreController.Middleware;

namespace TechStoreController
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            
            // HttpClient
            builder.Services.AddHttpClient();

            // ============================================
            // Infrastructure Layer (DAL) - DbContext
            // ============================================
            builder.Services.AddDbContext<TechStoreContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

<<<<<<< HEAD
            // Đăng ký Clerk Webhook Verifier
            var clerkWebhookSecret = builder.Configuration["Clerk:WebhookSecret"] 
                ?? throw new InvalidOperationException("Clerk:WebhookSecret is not configured");
            builder.Services.AddSingleton(new ClerkWebhookVerifier(clerkWebhookSecret));

            // Đăng ký UserService
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddControllers(options =>
            {
                // Disable model binding cho webhook endpoint để có thể đọc raw body
                options.ModelValidatorProviders.Clear();
            });
=======
            // ============================================
            // Infrastructure Layer (DAL) - Repositories
            // ============================================
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IBrandRepository, BrandRepository>();
            builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
            builder.Services.AddScoped<IAddressRepository, AddressRepository>();
            builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddScoped<ICommentReplyRepository, CommentReplyRepository>();
            builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
            builder.Services.AddScoped<IVoucherUsageRepository, VoucherUsageRepository>();

            // ============================================
            // Application Layer (BAL) - Services
            // ============================================
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IBrandService, BrandService>();
            builder.Services.AddScoped<IProductImageService, ProductImageService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IVoucherService, VoucherService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();

            // ============================================
            // API Layer - Controllers & Swagger
            // ============================================
            builder.Services.AddControllers();
            
>>>>>>> origin/dev
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TechStore API",
                    Version = "v1",
                    Description = "API documentation for TechStore Mobile Backend",
                    Contact = new OpenApiContact
                    {
                        Name = "TechStore Team"
                    }
                });

                // Include XML comments if available
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            // ============================================
            // JWT Authentication Configuration
            // ============================================
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWTTokenGenerationThatShouldBeAtLeast32CharactersLong";
            var issuer = jwtSettings["Issuer"] ?? "TechStore";
            var audience = jwtSettings["Audience"] ?? "TechStoreUsers";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // ============================================
            // Authorization Policies
            // ============================================
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
                options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Staff", "Admin"));
            });

            // CORS configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

<<<<<<< HEAD
            // Configure the HTTP request pipeline.
            
            // Enable request buffering cho webhook endpoint - PHẢI ĐẶT TRƯỚC CÁC MIDDLEWARE KHÁC
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/api/webhook/clerk"))
                {
                    // Enable buffering với buffer size lớn để đảm bảo có thể đọc body
                    context.Request.EnableBuffering(bufferLimit: 10485760); // 10MB
                }
                await next();
            });

            // Enable Swagger in all environments (including production)
=======
            // ============================================
            // Configure the HTTP request pipeline
            // ============================================
            
            // Request ID middleware (should be first)
            app.UseMiddleware<RequestIdMiddleware>();
            
            // Global exception handling middleware
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Enable Swagger in all environments
>>>>>>> origin/dev
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechStore API v1");
                c.RoutePrefix = "swagger"; // Swagger UI will be available at /swagger
                c.DisplayRequestDuration();
            });
            
            // Auto apply EF Core migrations at startup
            ApplyPendingMigrations(app);

            app.UseHttpsRedirection();

            // CORS
            app.UseCors("AllowAll");

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map root endpoint
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

            app.MapControllers();

            app.Run();
        }

        private static void ApplyPendingMigrations(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TechStoreContext>();
            dbContext.Database.Migrate();
        }
    }
}

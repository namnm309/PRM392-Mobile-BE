using BAL.Services;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

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
            // Cấu hình DbContext
            builder.Services.AddDbContext<TechStoreContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

            var app = builder.Build();

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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShoeStore.Application.Interfaces;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Application.Services;
using ShoeStore.Infrastructure.Persistence;
using ShoeStore.Infrastructure.Persistence.Interceptors;
using ShoeStore.Infrastructure.Services;

namespace ShoeStore.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ShoeStoreDb");
            services.AddDbContext<ShoeStoreDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<IReceiptService, ReceiptService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOrderService, OrderService>();
        }
    }
}

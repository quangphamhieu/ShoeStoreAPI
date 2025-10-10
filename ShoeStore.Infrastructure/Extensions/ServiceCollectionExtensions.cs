using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ShoeStoreDb");
            services.AddDbContext<ShoeStoreDbContext>(options => options.UseSqlServer(connectionString));
        }
    }
}

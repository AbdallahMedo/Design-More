using MechanicalDesigns.Infrastructure.Persistence;
using MechanicalDesigns.Infrastructure.Authentication;
using MechanicalDesigns.Application.Authentication;
using MechanicalDesigns.Application.Categories;
using MechanicalDesigns.Infrastructure.Categories;
using MechanicalDesigns.Application.Products;
using MechanicalDesigns.Infrastructure.Products;
using MechanicalDesigns.Application.Reviews;
using MechanicalDesigns.Infrastructure.Reviews;
using MechanicalDesigns.Application.Cart;
using MechanicalDesigns.Infrastructure.ShoppingCarts;
using MechanicalDesigns.Application.Orders;
using MechanicalDesigns.Infrastructure.Orders;
using MechanicalDesigns.Application.CustomOrders;
using MechanicalDesigns.Infrastructure.CustomOrders;
using MechanicalDesigns.Application.Advertisements;
using MechanicalDesigns.Infrastructure.Advertisements;
using MechanicalDesigns.Application.Admin;
using MechanicalDesigns.Infrastructure.Admin;
using MechanicalDesigns.Application.Storage;
using MechanicalDesigns.Infrastructure.Storage;
using MechanicalDesigns.Application.Email;
using MechanicalDesigns.Infrastructure.Email;
using MechanicalDesigns.Application.Delivery;
using MechanicalDesigns.Infrastructure.Delivery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicalDesigns.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductImageService, ProductImageService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomOrderService, CustomOrderService>();
        services.AddScoped<IAdvertisementService, AdvertisementService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IGovernorateDeliveryFeeService, GovernorateDeliveryFeeService>();
        services.AddScoped<AdminBootstrapper>();

        return services;
    }
}

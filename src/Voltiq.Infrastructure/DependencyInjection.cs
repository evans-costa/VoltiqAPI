using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Domain.Interfaces.Repositories.RefreshToken;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Exceptions.Resources;
using Voltiq.Infrastructure.Auth;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Interceptors;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Infrastructure.Persistence.Repositories.Budget;
using Voltiq.Infrastructure.Persistence.Repositories.Client;
using Voltiq.Infrastructure.Persistence.Repositories.Material;
using Voltiq.Infrastructure.Persistence.Repositories.TokenRepository;
using Voltiq.Infrastructure.Persistence.Repositories.User;

namespace Voltiq.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        AddRepositories(services);
        AddDatabase(services, configuration);
        AddJwtAuthentication(services, configuration);
        AddAuthServices(services);
        AddCryptography(services);
    }

    private static void AddCryptography(IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
    }

    private static void AddAuthServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<UserRepository>();
        services.AddScoped<IUserReadOnlyRepository>(sp => sp.GetRequiredService<UserRepository>());
        services.AddScoped<IUserWriteOnlyRepository>(sp => sp.GetRequiredService<UserRepository>());

        services.AddScoped<RefreshTokenRepository>();
        services.AddScoped<IRefreshTokenReadOnlyRepository>(sp => sp.GetRequiredService<RefreshTokenRepository>());
        services.AddScoped<IRefreshTokenWriteOnlyRepository>(sp => sp.GetRequiredService<RefreshTokenRepository>());

        services.AddScoped<ClientRepository>();
        services.AddScoped<IClientReadOnlyRepository>(sp => sp.GetRequiredService<ClientRepository>());
        services.AddScoped<IClientWriteOnlyRepository>(sp => sp.GetRequiredService<ClientRepository>());
        services.AddScoped<IClientUpdateOnlyRepository>(sp => sp.GetRequiredService<ClientRepository>());

        services.AddScoped<MaterialRepository>();
        services.AddScoped<IMaterialReadOnlyRepository>(sp => sp.GetRequiredService<MaterialRepository>());
        services.AddScoped<IMaterialWriteOnlyRepository>(sp => sp.GetRequiredService<MaterialRepository>());
        services.AddScoped<IMaterialUpdateOnlyRepository>(sp => sp.GetRequiredService<MaterialRepository>());

        services.AddScoped<BudgetRepository>();
        services.AddScoped<IBudgetReadOnlyRepository>(sp => sp.GetRequiredService<BudgetRepository>());
        services.AddScoped<IBudgetWriteOnlyRepository>(sp => sp.GetRequiredService<BudgetRepository>());
        services.AddScoped<IBudgetUpdateOnlyRepository>(sp => sp.GetRequiredService<BudgetRepository>());
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddSingleton<SoftDeleteInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) => options
            .UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));
    }

    private static void AddJwtAuthentication(IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
                        ?? throw new InvalidOperationException(
                            "JwtSettings:SecretKey is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";

                        await context.Response.WriteAsJsonAsync(new
                        {
                            title = ResourceErrorMessages.TITULO_NAO_AUTORIZADO,
                            status = StatusCodes.Status401Unauthorized,
                            instance = context.Request.Path.Value
                        });
                    }
                };
            });

        services.AddAuthorizationBuilder();
    }
}

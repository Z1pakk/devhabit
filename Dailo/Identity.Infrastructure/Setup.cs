using System.Text;
using Identity.Api;
using Identity.Application;
using Identity.Application.Configuration;
using Identity.Application.Persistence;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Infrastructure.Database;
using Identity.Infrastructure.Database.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Configuration;
using SharedKernel.CQRS;
using SharedKernel.Endpoint;
using SharedKernel.Persistence;

namespace Identity.Infrastructure;

public static class Setup
{
    public const string IdentityDbConnectionString = "IdentityPostgresConnectionString";

    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString(IdentityDbConnectionString);

        services.AddDbContext<IdentityDbContext>(opt =>
            opt.UseNpgsql(
                    connectionString,
                    b =>
                    {
                        b.MigrationsAssembly(AssemblyReference.Assembly)
                            .MigrationsHistoryTable(
                                HistoryRepository.DefaultTableName,
                                IdentitySchema.Name
                            );
                        // Enable retry on failure for transient errors
                        b.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null
                        );

                        // Set command timeout for long-running queries
                        b.CommandTimeout(60);
                    }
                )
                .UseSnakeCaseNamingConvention()
        );

        services.AddScoped<IIdentityDbContext>(sp => sp.GetRequiredService<IdentityDbContext>());

        services
            .AddIdentity<User, Role>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IDataSeeder, RoleSeeder>();

        services.AddValidateOptions<JwtAuthOptions>();
        var jwtOptions = services.GetOptions<JwtAuthOptions>();

        services.AddScoped<ITokenProvider, TokenProvider>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Key)
                    ),
                };
            });

        services.AddAuthorization();

        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.AddEndpoints(assemblies: IdentityApiRoot.Assembly);

        services.AddHandlerAssembly<IIdentityApplicationRoot>();

        return services;
    }
}

using Botify.Application.Common.Interfaces;
using Botify.Application.Services.Auths;
using Botify.Infrastructure.Authentication;
using Botify.Infrastructure.Identity;
using Botify.Infrastructure.Persistence;
using Botify.Infrastructure.Persistence.Interceptors;
using Botify.Infrastructure.Services.FileServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureRegisterServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp,options) =>
        {
            
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        services.AddAuthenticationJwtToken(configuration);


        services.AddScoped<ITokenProvider, JwtTokenProvider>();
        services.AddScoped<IAuthServices, AuthService>();

        services.AddFileService();
        return services;
    }

    private static IServiceCollection AddAuthenticationJwtToken(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
       {
           options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
           options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
       }).AddJwtBearer(options =>
       {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Cookies["access_token"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
           var jwtOptions = configuration.GetSection("JWTOptions");
           options.TokenValidationParameters = new()
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = jwtOptions["Issuer"],
               ValidAudience = jwtOptions["Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions["SecurityKey"]!))
           };
       });

        return services;
    }

    private static IServiceCollection AddFileService(this IServiceCollection services)
    {
        services.AddSingleton<IFileValidator, FileValidator>();
        services.AddScoped<IFileService>(sp =>
            new LocalFileService("uploads", sp.GetRequiredService<IFileValidator>(), sp.GetRequiredService<ILogger<LocalFileService>>()));
        return services;
    }
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = new[] { "MERCHANT", "ADMIN", "USER" };

            foreach (var role in roles)
            {
                var exists = await roleManager.RoleExistsAsync(role);
                if (!exists)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
    }
}
using Azure.Identity;
using ebr_powerbi.Application.Common.Interfaces;
using ebr_powerbi.Infrastructure.Data;
using ebr_powerbi.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
            options.AddOperationTransformer<IdentityApiOperationTransformer>();
        });

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                // Cookie auth + Angular dev server on another port requires explicit origins and AllowCredentials.
                // AllowAnyOrigin() is incompatible with credentials and breaks browsers (login / info XHR).
                if (builder.Environment.IsDevelopment())
                {
                    policy
                        .SetIsOriginAllowed(static origin =>
                        {
                            if (string.IsNullOrEmpty(origin)) return false;
                            try
                            {
                                var uri = new Uri(origin);
                                return string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                                       || uri.Host == "127.0.0.1";
                            }
                            catch
                            {
                                return false;
                            }
                        })
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
                else
                {
                    policy.AllowAnyHeader().AllowAnyMethod();
                }
            });
        });
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}

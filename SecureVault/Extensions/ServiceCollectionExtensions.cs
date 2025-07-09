using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SecureVault.Services;
using SecureVault.Commands;

namespace SecureVault.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddVaultCliServices(this IServiceCollection services)
        {
            // Configuration services
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            
            // Logging services
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Vault services
            services.AddScoped<IVaultService, VaultService>();
            
            // Command services
            services.AddScoped<InitCommand>();
            services.AddScoped<UnsealCommand>();
            services.AddScoped<LoginCommand>();
            services.AddScoped<WriteSecretCommand>();
            services.AddScoped<ReadSecretCommand>();
            
            return services;
        }
    }
}
using Microsoft.Extensions.Configuration;
using SecureVault.Models;
using System.Text.Json;

namespace SecureVault.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly string _configFilePath;

        public ConfigurationService()
        {
            _configFilePath = "vault-cli.json";
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(_configFilePath, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("VAULT_")
                .AddCommandLine(Environment.GetCommandLineArgs());

            _configuration = builder.Build();
        }

        public VaultConfiguration GetConfiguration()
        {
            var config = new VaultConfiguration();
            
            // Bind configuration from all sources
            _configuration.GetSection("VaultConfiguration").Bind(config);
            
            // Override with environment variables if present
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VAULT_ADDR")))
                config.Endpoint = Environment.GetEnvironmentVariable("VAULT_ADDR")!;
            
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VAULT_SKIP_VERIFY")))
                config.SkipTlsVerification = bool.Parse(Environment.GetEnvironmentVariable("VAULT_SKIP_VERIFY")!);

            return config;
        }

        public void SaveConfiguration(VaultConfiguration configuration)
        {
            var configWrapper = new { VaultConfiguration = configuration };
            var json = JsonSerializer.Serialize(configWrapper, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(_configFilePath, json);
        }
    }
}
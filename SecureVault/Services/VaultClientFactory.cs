using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using SecureVault.Models;

namespace SecureVault.Services
{
    public class VaultClientFactory
    {
        public static IVaultClient CreateClient(VaultConfiguration config, string? token = null)
        {
            var authMethod = string.IsNullOrEmpty(token)
                ? null
                : new TokenAuthMethodInfo(token);
                
            var settings = new VaultClientSettings(config.Endpoint, authMethod)
            {
                VaultServiceTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds),
                UseVaultTokenHeaderInsteadOfAuthorizationHeader = true
            };

            // Note: TLS verification handling would need to be implemented at HttpClient level
            // For now, we'll document this as a limitation that needs to be addressed
            // in production environments through proper certificate management

            return new VaultClient(settings);
        }

        public static IVaultClient CreateUnauthenticatedClient(VaultConfiguration config)
        {
            return CreateClient(config, null);
        }
    }
}
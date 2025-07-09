using SecureVault.Models;

namespace SecureVault.Services
{
    public interface IConfigurationService
    {
        VaultConfiguration GetConfiguration();
        void SaveConfiguration(VaultConfiguration configuration);
    }
}
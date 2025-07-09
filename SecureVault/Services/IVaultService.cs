using SecureVault.Models;
using VaultSharp.V1.SystemBackend;

namespace SecureVault.Services
{
    public interface IVaultService
    {
        Task<InitializationData> InitializeAsync(int secretShares, int secretThreshold);
        Task<bool> UnsealAsync(IEnumerable<string> keys);
        Task<string> LoginAsync(string token);
        Task WriteSecretAsync(string path, string key, string value);
        Task<string?> ReadSecretAsync(string path, string key);
        Task<SealStatus> GetSealStatusAsync();
        Task<bool> GetInitializationStatusAsync();
        Task<bool> IsAuthenticatedAsync();
    }
}
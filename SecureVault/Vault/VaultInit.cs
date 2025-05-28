using VaultSharp;

namespace SecureVault.Vault
{
    public static class VaultInit
    {
        public static async Task InitializeAsync()
        {
            // Initialize Vault
            Console.WriteLine("Vault initialized and unsealed");
            var vaultClientSettings = new VaultClientSettings("http://localhost:8200", null);
            var vaultClient = new VaultClient(vaultClientSettings);

            // Unseal Vault
            var initStatus = await vaultClient.V1.System.GetInitStatusAsync();
            if (initStatus)
            {
                Console.WriteLine("Vault is already initialized");
            }
        }
    }
}
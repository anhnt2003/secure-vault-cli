using VaultSharp;

namespace SecureVault.Vault
{
    public static class VaultSecret
    {
        public static async Task WriteSecretAsync(IVaultClient vaultClient, string path, string key, string value)
        {
            // Write secret
            Console.WriteLine("Writing secret...");
            try
            {
                var secretData = new Dictionary<string, object>
                {
                    { key, value }
                };

                await vaultClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(
                    path: path,
                    data: secretData,
                    mountPoint: "secret"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing secret: {ex.Message}");
            }
        }

        public static async Task<string?> ReadSecretAsync(IVaultClient vaultClient, string path, string key)
        {
            // Read secret
            Console.WriteLine("Reading secret...");
            try
            {
                var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                    path: path,
                    mountPoint: "secret"
                );

                if (secret.Data.Data != null && secret.Data.Data.ContainsKey(key))
                {
                    var secretValue = secret.Data.Data[key];
                    return secretValue.ToString();
                }
                else
                {
                    Console.WriteLine("Secret not found");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading secret: {ex.Message}");
                return null;
            }
        }
    }
}
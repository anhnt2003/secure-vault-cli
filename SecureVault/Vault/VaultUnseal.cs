using VaultSharp;

namespace SecureVault.Vault
{
    public static class VaultUnseal
    {
        public static async Task UnsealAsync(IVaultClient vaultClient, IList<string> unsealKeys)
        {
            // Unseal Vault
            Console.WriteLine("Unsealing Vault...");
            var unsealStatus = await vaultClient.V1.System.GetSealStatusAsync();
            if (!unsealStatus.Sealed)
            {
                Console.WriteLine("Vault is already unsealed");
                return;
            }

            foreach (var key in unsealKeys)
            {
                var unsealResult = await vaultClient.V1.System.UnsealAsync(key);
                if (unsealResult.Sealed)
                {
                    Console.WriteLine("Vault is still sealed");
                }
                else
                {
                    Console.WriteLine("Vault is unsealed");
                    return;
                }
            }
        }
    }
}
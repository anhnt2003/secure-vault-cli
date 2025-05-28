using VaultSharp;

namespace SecureVault.Vault
{
    public static class VaultSeal
    {
        public static async Task SealAsync(IVaultClient vaultClient)
        {
            // Seal Vault
            Console.WriteLine("Sealing Vault...");
            var sealStatus = await vaultClient.V1.System.GetSealStatusAsync();
            if (sealStatus.Sealed)
            {
                Console.WriteLine("Vault is already sealed");
                return;
            }

            await vaultClient.V1.System.SealAsync();
        }
    }
}
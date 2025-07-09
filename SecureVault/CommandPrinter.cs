namespace SecureVault
{
    public static class CommandPrinter
    {
        public static void UsagePrinter()
        {
            Console.WriteLine("VaultCliCSharp - Vault CLI Management Tool");
            Console.WriteLine("Usage: dotnet run -- <command> [arguments]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  init [--shares N] [--threshold N]  Initialize Vault with key shares");
            Console.WriteLine("  unseal                             Unseal Vault using stored keys");
            Console.WriteLine("  login [--token TOKEN]              Authenticate with Vault");
            Console.WriteLine("  write <path> <key> <value>         Write a secret to Vault");
            Console.WriteLine("  read <path> <key>                  Read a secret from Vault");
            Console.WriteLine("  help                               Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run -- init --shares 5 --threshold 3");
            Console.WriteLine("  dotnet run -- unseal");
            Console.WriteLine("  dotnet run -- login --token hvs.XXXXXX");
            Console.WriteLine("  dotnet run -- write myapp/db password secret123");
            Console.WriteLine("  dotnet run -- read myapp/db password");
            Console.WriteLine();
            Console.WriteLine("Environment Variables:");
            Console.WriteLine("  VAULT_ADDR         Vault server address (default: http://127.0.0.1:8200)");
            Console.WriteLine("  VAULT_TOKEN        Vault authentication token");
            Console.WriteLine("  VAULT_SKIP_VERIFY  Skip TLS verification (default: false)");
        }
    }
}
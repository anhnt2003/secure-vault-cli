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
            Console.WriteLine("  init-unseal                    Initialize and unseal Vault");
            Console.WriteLine("  login                          Login and show token TTL");
            Console.WriteLine("  write-secret <path> <key>      Write a secret to Vault");
            Console.WriteLine("  read-secret <path> <key>       Read a secret from Vault");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run -- init-unseal");
            Console.WriteLine("  dotnet run -- login");
            Console.WriteLine("  dotnet run -- write-secret myapp/db password");
            Console.WriteLine("  dotnet run -- read-secret myapp/db password");
        }
    }
}
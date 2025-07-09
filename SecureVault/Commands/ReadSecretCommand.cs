using Microsoft.Extensions.Logging;
using SecureVault.Services;
using SecureVault.Utilities;

namespace SecureVault.Commands
{
    public class ReadSecretCommand(ILogger<ReadSecretCommand> logger, IVaultService vaultService) : ICommand
    {
        private readonly ILogger<ReadSecretCommand> _logger = logger;
        private readonly IVaultService _vaultService = vaultService;

        public string Name => "read";
        public string Description => "Read a secret from Vault";

        public async Task<int> ExecuteAsync(string[] args)
        {
            try
            {
                _logger.LogInformation("Reading secret from Vault");

                // Parse arguments: read <path> <key>
                if (args.Length < 2)
                {
                    Console.Error.WriteLine("Error: Insufficient arguments");
                    Console.Error.WriteLine("Usage: read <path> <key>");
                    Console.Error.WriteLine("Example: read myapp/db password");
                    return 2;
                }

                var path = args[0];
                var key = args[1];

                // Validate inputs
                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.Error.WriteLine("Error: Path cannot be empty");
                    return 2;
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    Console.Error.WriteLine("Error: Key cannot be empty");
                    return 2;
                }

                // Additional validation using InputValidator
                if (!InputValidator.IsValidPath(path))
                {
                    Console.Error.WriteLine("Error: Invalid path format");
                    Console.Error.WriteLine("Path should not contain '..' or invalid characters");
                    return 2;
                }

                if (!InputValidator.IsValidKey(key))
                {
                    Console.Error.WriteLine("Error: Invalid key format");
                    Console.Error.WriteLine("Key should contain only letters, numbers, underscores, and hyphens");
                    return 2;
                }

                Console.WriteLine($"Reading secret from path: {path}");
                Console.WriteLine($"Key: {key}");

                // Check if we're authenticated
                var isAuthenticated = await _vaultService.IsAuthenticatedAsync();
                if (!isAuthenticated)
                {
                    Console.Error.WriteLine("Error: Not authenticated with Vault");
                    Console.Error.WriteLine("Please run 'login' command first");
                    return 3;
                }

                // Read the secret
                var secretValue = await _vaultService.ReadSecretAsync(path, key);

                if (secretValue != null)
                {
                    Console.WriteLine("✅ Secret retrieved successfully!");
                    Console.WriteLine($"Path: {path}");
                    Console.WriteLine($"Key: {key}");
                    Console.WriteLine($"Value: {secretValue}");
                    return 0;
                }
                else
                {
                    Console.WriteLine("❌ Secret not found");
                    Console.WriteLine($"Path: {path}");
                    Console.WriteLine($"Key: {key}");
                    Console.WriteLine("The secret may not exist or you may not have permission to read it");
                    return 4;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read secret");
                Console.Error.WriteLine($"Error: {ex.Message}");
                
                // Provide helpful error messages
                if (ex.Message.Contains("permission denied"))
                {
                    Console.Error.WriteLine("Hint: Check if your token has read permissions for this path");
                }
                else if (ex.Message.Contains("authentication"))
                {
                    Console.Error.WriteLine("Hint: Please run 'login' command first");
                }
                else if (ex.Message.Contains("path"))
                {
                    Console.Error.WriteLine("Hint: Check if the path format is correct");
                }
                else if (ex.Message.Contains("not found"))
                {
                    Console.Error.WriteLine("Hint: The secret may not exist at the specified path");
                }
                
                return 4;
            }
        }
    }
}
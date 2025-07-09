using Microsoft.Extensions.Logging;
using SecureVault.Services;
using SecureVault.Utilities;

namespace SecureVault.Commands
{
    public class WriteSecretCommand : ICommand
    {
        private readonly ILogger<WriteSecretCommand> _logger;
        private readonly IVaultService _vaultService;

        public string Name => "write";
        public string Description => "Write a secret to Vault";

        public WriteSecretCommand(ILogger<WriteSecretCommand> logger, IVaultService vaultService)
        {
            _logger = logger;
            _vaultService = vaultService;
        }

        public async Task<int> ExecuteAsync(string[] args)
        {
            try
            {
                _logger.LogInformation("Writing secret to Vault");

                // Parse arguments: write <path> <key> <value>
                if (args.Length < 3)
                {
                    Console.Error.WriteLine("Error: Insufficient arguments");
                    Console.Error.WriteLine("Usage: write <path> <key> <value>");
                    Console.Error.WriteLine("Example: write myapp/db password secret123");
                    return 2;
                }

                var path = args[0];
                var key = args[1];
                var value = args[2];

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

                if (string.IsNullOrWhiteSpace(value))
                {
                    Console.Error.WriteLine("Error: Value cannot be empty");
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

                Console.WriteLine($"Writing secret to path: {path}");
                Console.WriteLine($"Key: {key}");
                Console.WriteLine("Value: [HIDDEN]");

                // Check if we're authenticated
                var isAuthenticated = await _vaultService.IsAuthenticatedAsync();
                if (!isAuthenticated)
                {
                    Console.Error.WriteLine("Error: Not authenticated with Vault");
                    Console.Error.WriteLine("Please run 'login' command first");
                    return 3;
                }

                // Write the secret
                await _vaultService.WriteSecretAsync(path, key, value);

                Console.WriteLine("✅ Secret written successfully!");
                Console.WriteLine($"Path: {path}");
                Console.WriteLine($"Key: {key}");

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write secret");
                Console.Error.WriteLine($"Error: {ex.Message}");
                
                // Provide helpful error messages
                if (ex.Message.Contains("permission denied"))
                {
                    Console.Error.WriteLine("Hint: Check if your token has write permissions for this path");
                }
                else if (ex.Message.Contains("authentication"))
                {
                    Console.Error.WriteLine("Hint: Please run 'login' command first");
                }
                else if (ex.Message.Contains("path"))
                {
                    Console.Error.WriteLine("Hint: Check if the path format is correct");
                }
                
                return 4;
            }
        }
    }
}
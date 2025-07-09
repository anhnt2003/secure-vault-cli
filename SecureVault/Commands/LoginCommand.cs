using Microsoft.Extensions.Logging;
using SecureVault.Services;

namespace SecureVault.Commands
{
    public class LoginCommand(ILogger<LoginCommand> logger, IVaultService vaultService) : ICommand
    {
        private readonly ILogger<LoginCommand> _logger = logger;
        private readonly IVaultService _vaultService = vaultService;

        public string Name => "login";
        public string Description => "Authenticate with Vault using a token";

        public async Task<int> ExecuteAsync(string[] args)
        {
            try
            {
                _logger.LogInformation("Attempting to authenticate with Vault");

                // Parse token from arguments or environment
                string? token = null;
                
                // Check command line arguments
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--token" && i + 1 < args.Length)
                    {
                        token = args[i + 1];
                        break;
                    }
                }

                // Check environment variable if not provided via command line
                if (string.IsNullOrEmpty(token))
                {
                    token = Environment.GetEnvironmentVariable("VAULT_TOKEN");
                }

                // Prompt for token if not provided
                if (string.IsNullOrEmpty(token))
                {
                    Console.Write("Enter Vault token: ");
                    token = Console.ReadLine();
                }

                if (string.IsNullOrEmpty(token))
                {
                    Console.Error.WriteLine("Error: No token provided");
                    Console.Error.WriteLine("Usage: login --token <token>");
                    Console.Error.WriteLine("   or: set VAULT_TOKEN environment variable");
                    return 2;
                }

                // Validate token format (basic validation)
                if (token.Length < 10)
                {
                    Console.Error.WriteLine("Error: Token appears to be too short");
                    return 2;
                }

                Console.WriteLine("Authenticating with Vault...");

                // Attempt login
                var authenticatedToken = await _vaultService.LoginAsync(token);

                Console.WriteLine("✅ Authentication successful!");
                Console.WriteLine($"Token saved securely");
                
                // Verify authentication by checking if we can access Vault
                var isAuthenticated = await _vaultService.IsAuthenticatedAsync();
                if (isAuthenticated)
                {
                    Console.WriteLine("✅ Token verification successful");
                }
                else
                {
                    Console.WriteLine("⚠️  Warning: Token saved but verification failed");
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed");
                Console.Error.WriteLine($"Error: {ex.Message}");
                
                // Provide helpful error messages based on common issues
                if (ex.Message.Contains("permission denied") || ex.Message.Contains("invalid token"))
                {
                    Console.Error.WriteLine("Hint: Check if the token is valid and has the required permissions");
                }
                else if (ex.Message.Contains("connection") || ex.Message.Contains("network"))
                {
                    Console.Error.WriteLine("Hint: Check if Vault server is running and accessible");
                }
                
                return 3;
            }
        }
    }
}
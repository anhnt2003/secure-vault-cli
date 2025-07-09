using Microsoft.Extensions.Logging;
using SecureVault.Services;
using SecureVault.Utilities;

namespace SecureVault.Commands
{
    public class InitCommand : ICommand
    {
        private readonly ILogger<InitCommand> _logger;
        private readonly IVaultService _vaultService;

        public string Name => "init";
        public string Description => "Initialize Vault with key shares and threshold";

        public InitCommand(ILogger<InitCommand> logger, IVaultService vaultService)
        {
            _logger = logger;
            _vaultService = vaultService;
        }

        public async Task<int> ExecuteAsync(string[] args)
        {
            try
            {
                // Parse arguments
                var parsedArgs = ArgumentParser.ParseInitArguments(args);
                var shares = parsedArgs.GetValueOrDefault("shares", 5);
                var threshold = parsedArgs.GetValueOrDefault("threshold", 3);

                _logger.LogInformation("Initializing Vault with {Shares} shares and {Threshold} threshold", shares, threshold);

                // Validate arguments
                if (shares < 1 || shares > 255)
                {
                    Console.Error.WriteLine("Error: Shares must be between 1 and 255");
                    return 2;
                }

                if (threshold < 1 || threshold > shares)
                {
                    Console.Error.WriteLine("Error: Threshold must be between 1 and the number of shares");
                    return 2;
                }

                // Check if Vault is already initialized
                var isInitialized = await _vaultService.GetInitializationStatusAsync();
                if (isInitialized)
                {
                    Console.WriteLine("Vault is already initialized");
                    return 0;
                }

                // Initialize Vault
                var initData = await _vaultService.InitializeAsync(shares, threshold);

                // Display results
                Console.WriteLine("Vault initialized successfully!");
                Console.WriteLine($"Root Token: {initData.RootToken}");
                Console.WriteLine("Unseal Keys:");
                for (int i = 0; i < initData.UnsealKeys.Count; i++)
                {
                    Console.WriteLine($"  Key {i + 1}: {initData.UnsealKeys[i]}");
                }
                Console.WriteLine();
                Console.WriteLine("IMPORTANT: Save these keys and token securely!");
                Console.WriteLine($"Initialization data has been saved to: {Environment.CurrentDirectory}/vault_init.json");

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Vault");
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 4;
            }
        }
    }
}
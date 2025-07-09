using Microsoft.Extensions.Logging;
using SecureVault.Services;
using SecureVault.Models;
using System.Text.Json;

namespace SecureVault.Commands
{
    public class UnsealCommand : ICommand
    {
        private readonly ILogger<UnsealCommand> _logger;
        private readonly IVaultService _vaultService;
        private readonly IConfigurationService _configService;

        public string Name => "unseal";
        public string Description => "Unseal Vault using stored keys";

        public UnsealCommand(ILogger<UnsealCommand> logger, IVaultService vaultService, IConfigurationService configService)
        {
            _logger = logger;
            _vaultService = vaultService;
            _configService = configService;
        }

        public async Task<int> ExecuteAsync(string[] args)
        {
            try
            {
                _logger.LogInformation("Attempting to unseal Vault");

                // Check seal status first
                var sealStatus = await _vaultService.GetSealStatusAsync();
                if (!sealStatus.Sealed)
                {
                    Console.WriteLine("Vault is already unsealed");
                    return 0;
                }

                Console.WriteLine($"Vault is sealed. Progress: {sealStatus.Progress}/{sealStatus.SecretThreshold}");

                // Load initialization data to get unseal keys
                var config = _configService.GetConfiguration();
                var initFilePath = config.InitFile;

                if (!File.Exists(initFilePath))
                {
                    Console.Error.WriteLine($"Error: Initialization file not found at {initFilePath}");
                    Console.Error.WriteLine("Please run 'init' command first or provide the correct path to the initialization file.");
                    return 4;
                }

                InitializationData? initData;
                try
                {
                    var json = await File.ReadAllTextAsync(initFilePath);
                    initData = JsonSerializer.Deserialize<InitializationData>(json);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error reading initialization file: {ex.Message}");
                    return 4;
                }

                if (initData?.UnsealKeys == null || initData.UnsealKeys.Count == 0)
                {
                    Console.Error.WriteLine("Error: No unseal keys found in initialization file");
                    return 4;
                }

                Console.WriteLine($"Found {initData.UnsealKeys.Count} unseal keys");
                Console.WriteLine("Attempting to unseal Vault...");

                // Attempt to unseal
                var success = await _vaultService.UnsealAsync(initData.UnsealKeys);

                if (success)
                {
                    Console.WriteLine("✅ Vault unsealed successfully!");
                    
                    // Verify seal status
                    var finalStatus = await _vaultService.GetSealStatusAsync();
                    Console.WriteLine($"Seal Status: {(finalStatus.Sealed ? "Sealed" : "Unsealed")}");
                    
                    return 0;
                }
                else
                {
                    Console.Error.WriteLine("❌ Failed to unseal Vault with provided keys");
                    
                    // Show current status
                    var currentStatus = await _vaultService.GetSealStatusAsync();
                    Console.WriteLine($"Current progress: {currentStatus.Progress}/{currentStatus.SecretThreshold}");
                    
                    return 4;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unseal Vault");
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 4;
            }
        }
    }
}
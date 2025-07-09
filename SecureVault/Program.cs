using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SecureVault.Extensions;
using SecureVault.Services;
using SecureVault.Exceptions;
using SecureVault.Commands;

namespace SecureVault;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            // Set up dependency injection
            var services = new ServiceCollection();
            services.AddVaultCliServices();
            
            using var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var configService = serviceProvider.GetRequiredService<IConfigurationService>();

            logger.LogInformation("VaultCliCSharp - Vault CLI Management Tool");
            
            // For Phase 1, we'll implement basic command routing
            // In Phase 2, we'll integrate System.CommandLine properly
            if (args.Length == 0)
            {
                CommandPrinter.UsagePrinter();
                return 0;
            }

            var command = args[0].ToLowerInvariant();
            
            // Get remaining arguments (excluding the command)
            var commandArgs = args.Skip(1).ToArray();
            
            switch (command)
            {
                case "init":
                    var initCommand = serviceProvider.GetRequiredService<InitCommand>();
                    return await initCommand.ExecuteAsync(commandArgs);
                    
                case "unseal":
                    var unsealCommand = serviceProvider.GetRequiredService<UnsealCommand>();
                    return await unsealCommand.ExecuteAsync(commandArgs);
                    
                case "login":
                    var loginCommand = serviceProvider.GetRequiredService<LoginCommand>();
                    return await loginCommand.ExecuteAsync(commandArgs);
                    
                case "write":
                    var writeCommand = serviceProvider.GetRequiredService<WriteSecretCommand>();
                    return await writeCommand.ExecuteAsync(commandArgs);
                    
                case "read":
                    var readCommand = serviceProvider.GetRequiredService<ReadSecretCommand>();
                    return await readCommand.ExecuteAsync(commandArgs);
                    
                case "help":
                case "--help":
                case "-h":
                    CommandPrinter.UsagePrinter();
                    return 0;
                    
                default:
                    logger.LogError("Unknown command: {Command}", command);
                    CommandPrinter.UsagePrinter();
                    return 1;
            }
        }
        catch (ConfigurationException ex)
        {
            Console.Error.WriteLine($"Configuration error: {ex.Message}");
            return 5;
        }
        catch (VaultCliException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return 1;
        }
    }
}
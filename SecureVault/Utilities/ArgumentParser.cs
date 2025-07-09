using System.CommandLine;
using System.CommandLine.Parsing;

namespace SecureVault.Utilities
{
    public static class ArgumentParser
    {
        public static RootCommand CreateRootCommand()
        {
            var rootCommand = new RootCommand("VaultCliCSharp - Vault CLI Management Tool");

            // Init command
            var initCommand = new Command("init", "Initialize Vault with configurable key shares and threshold");
            var sharesOption = new Option<int>("--shares", () => 5, "Number of key shares");
            var thresholdOption = new Option<int>("--threshold", () => 3, "Number of keys required to unseal");
            initCommand.AddOption(sharesOption);
            initCommand.AddOption(thresholdOption);

            // Unseal command
            var unsealCommand = new Command("unseal", "Unseal Vault using stored keys");

            // Login command
            var loginCommand = new Command("login", "Authenticate with Vault using a token");
            var tokenOption = new Option<string>("--token", "Vault authentication token");
            loginCommand.AddOption(tokenOption);

            // Write secret command
            var writeCommand = new Command("write", "Write a secret to Vault");
            var writePathArgument = new Argument<string>("path", "Path where the secret will be stored");
            var writeKeyArgument = new Argument<string>("key", "Key name for the secret");
            var writeValueArgument = new Argument<string>("value", "Secret value");
            writeCommand.AddArgument(writePathArgument);
            writeCommand.AddArgument(writeKeyArgument);
            writeCommand.AddArgument(writeValueArgument);

            // Read secret command
            var readCommand = new Command("read", "Read a secret from Vault");
            var readPathArgument = new Argument<string>("path", "Path where the secret is stored");
            var readKeyArgument = new Argument<string>("key", "Key name for the secret");
            readCommand.AddArgument(readPathArgument);
            readCommand.AddArgument(readKeyArgument);

            // Add commands to root
            rootCommand.AddCommand(initCommand);
            rootCommand.AddCommand(unsealCommand);
            rootCommand.AddCommand(loginCommand);
            rootCommand.AddCommand(writeCommand);
            rootCommand.AddCommand(readCommand);

            return rootCommand;
        }

        public static ParseResult ParseArguments(string[] args)
        {
            var rootCommand = CreateRootCommand();
            return rootCommand.Parse(args);
        }

        public static Dictionary<string, int> ParseInitArguments(string[] args)
        {
            var result = new Dictionary<string, int>();
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--shares" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int shares))
                        result["shares"] = shares;
                }
                else if (args[i] == "--threshold" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int threshold))
                        result["threshold"] = threshold;
                }
            }
            
            return result;
        }
    }
}
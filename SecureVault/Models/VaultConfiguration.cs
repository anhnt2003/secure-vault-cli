namespace SecureVault.Models
{
    public class VaultConfiguration
    {
        // Endpoint to connect to Vault
        public string Endpoint { get; set; } = string.Empty;
        // File to store token
        public string TokenFile { get; set; } = string.Empty;
        // File to store initialization data
        public string InitFile { get; set; } = string.Empty;
        // Timeout for Vault operations
        public int TimeoutSeconds { get; set; } = 30;
        // Skip TLS verification
        public bool SkipTlsVerification { get; set; } = false;
    }
}
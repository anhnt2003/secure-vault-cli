namespace SecureVault.Models
{
    public class VaultConfiguration
    {
        public string Endpoint { get; set; } = "http://127.0.0.1:8200";
        public string TokenFile { get; set; } = ".token";
        public string InitFile { get; set; } = "vault_init.json";
        public int TimeoutSeconds { get; set; } = 30;
        public bool SkipTlsVerification { get; set; } = false;
        public int RetryCount { get; set; } = 3;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);
    }
}
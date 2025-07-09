namespace SecureVault.Models
{
    public class InitializationData
    {
        public string RootToken { get; set; } = string.Empty;
        public List<string> UnsealKeys { get; set; } = [];
        public int SecretShares { get; set; }
        public int SecretThreshold { get; set; }
        public DateTime InitializedAt { get; set; }
    }
}
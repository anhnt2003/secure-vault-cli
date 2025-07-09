namespace SecureVault.Exceptions
{
    public abstract class VaultCliException : Exception
    {
        protected VaultCliException(string message) : base(message) { }
        protected VaultCliException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class VaultOperationException : VaultCliException
    {
        public string Operation { get; }
        public int? StatusCode { get; }

        public VaultOperationException(string operation, string message, Exception? innerException = null)
            : base($"Vault operation '{operation}' failed: {message}", innerException!)
        {
            Operation = operation;
        }

        public VaultOperationException(string operation, string message, int statusCode, Exception? innerException = null)
            : base($"Vault operation '{operation}' failed: {message}", innerException!)
        {
            Operation = operation;
            StatusCode = statusCode;
        }
    }

    public class InitializationException(string message, Exception? innerException = null) : VaultOperationException("Initialize", message, innerException)
    {
    }

    public class AuthenticationException(string message, Exception? innerException = null) : VaultOperationException("Authentication", message, innerException)
    {
    }

    public class SecretOperationException(string operation, string path, string message, Exception? innerException = null) : VaultOperationException(operation, $"Path '{path}': {message}", innerException)
    {
        public string Path { get; } = path;
    }

    public class ConfigurationException(string message, Exception? innerException = null) : VaultCliException($"Configuration error: {message}", innerException!)
    {
    }

    public class ValidationException(string field, string message, Exception? innerException = null) : VaultCliException($"Validation error for '{field}': {message}", innerException!)
    {
        public string Field { get; } = field;
    }
}
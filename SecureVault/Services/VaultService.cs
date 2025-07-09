using Microsoft.Extensions.Logging;
using SecureVault.Exceptions;
using SecureVault.Models;
using SecureVault.Utilities;
using System.Text.Json;
using VaultSharp;
using VaultSharp.V1.SystemBackend;

namespace SecureVault.Services
{
    public class VaultService(ILogger<VaultService> logger, IConfigurationService configService) : IVaultService
    {
        private readonly ILogger<VaultService> _logger = logger;
        private readonly VaultConfiguration _config = configService.GetConfiguration();
        private IVaultClient? _vaultClient;

        public async Task<InitializationData> InitializeAsync(int secretShares, int secretThreshold)
        {
            try
            {
                _logger.LogInformation("Initializing Vault with {Shares} shares and {Threshold} threshold",
                    secretShares, secretThreshold);

                var client = VaultClientFactory.CreateUnauthenticatedClient(_config);
                
                // Check if already initialized
                var initStatus = await client.V1.System.GetInitStatusAsync();
                if (initStatus)
                {
                    throw new InitializationException("Vault is already initialized");
                }

                // For now, we'll create a placeholder implementation
                // This will need to be updated with the correct VaultSharp API call
                // TODO: Implement proper Vault initialization using VaultSharp
                
                var initData = new InitializationData
                {
                    RootToken = "placeholder-root-token",
                    UnsealKeys = new List<string> { "key1", "key2", "key3" },
                    SecretShares = secretShares,
                    SecretThreshold = secretThreshold,
                    InitializedAt = DateTime.UtcNow
                };

                // Save initialization data securely
                await SaveInitializationDataAsync(initData);

                _logger.LogInformation("Vault initialized successfully (placeholder implementation)");
                return initData;
            }
            catch (Exception ex) when (!(ex is InitializationException))
            {
                _logger.LogError(ex, "Failed to initialize Vault");
                throw new InitializationException($"Initialization failed: {ex.Message}", ex);
            }
        }

        public async Task<bool> UnsealAsync(IEnumerable<string> keys)
        {
            try
            {
                _logger.LogInformation("Attempting to unseal Vault");

                var client = VaultClientFactory.CreateUnauthenticatedClient(_config);
                
                // Check current seal status
                var sealStatus = await client.V1.System.GetSealStatusAsync();
                if (!sealStatus.Sealed)
                {
                    _logger.LogInformation("Vault is already unsealed");
                    return true;
                }

                // Unseal with provided keys
                foreach (var key in keys)
                {
                    var unsealResponse = await client.V1.System.UnsealAsync(key);
                    
                    if (!unsealResponse.Sealed)
                    {
                        _logger.LogInformation("Vault unsealed successfully");
                        return true;
                    }
                    
                    _logger.LogDebug("Unseal progress: {Progress}/{Total}", 
                        unsealResponse.Progress, unsealResponse.SecretThreshold);
                }

                _logger.LogWarning("Vault remains sealed after all keys provided");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unseal Vault");
                throw new VaultOperationException("Unseal", $"Unseal operation failed: {ex.Message}", ex);
            }
        }

        public async Task<string> LoginAsync(string token)
        {
            try
            {
                _logger.LogInformation("Authenticating with Vault");

                var client = VaultClientFactory.CreateClient(_config, token);
                
                // Verify token by looking up self
                var tokenInfo = await client.V1.Auth.Token.LookupSelfAsync();
                
                if (tokenInfo?.Data == null)
                {
                    throw new AuthenticationException("Invalid token or token lookup failed");
                }

                // Save token securely
                SecurityHelper.SaveSecureToken(token, _config.TokenFile);
                
                // Update client reference
                _vaultClient = client;

                _logger.LogInformation("Authentication successful");
                return token;
            }
            catch (Exception ex) when (!(ex is AuthenticationException))
            {
                _logger.LogError(ex, "Authentication failed");
                throw new AuthenticationException($"Login failed: {ex.Message}", ex);
            }
        }

        public async Task WriteSecretAsync(string path, string key, string value)
        {
            try
            {
                _logger.LogInformation("Writing secret to path: {Path}", path);

                // Validate inputs
                if (!InputValidator.IsValidPath(path))
                    throw new ValidationException("path", "Invalid path format");
                
                if (!InputValidator.IsValidKey(key))
                    throw new ValidationException("key", "Invalid key format");

                var client = await GetAuthenticatedClientAsync();
                
                var secretData = new Dictionary<string, object>
                {
                    { key, value }
                };

                await client.V1.Secrets.KeyValue.V2.WriteSecretAsync(
                    path: path,
                    data: secretData,
                    mountPoint: "secret"
                );

                _logger.LogInformation("Secret written successfully to {Path}", path);
            }
            catch (Exception ex) when (!(ex is ValidationException || ex is AuthenticationException))
            {
                _logger.LogError(ex, "Failed to write secret to {Path}", path);
                throw new SecretOperationException("Write", path, $"Write operation failed: {ex.Message}", ex);
            }
        }

        public async Task<string?> ReadSecretAsync(string path, string key)
        {
            try
            {
                _logger.LogInformation("Reading secret from path: {Path}", path);

                // Validate inputs
                if (!InputValidator.IsValidPath(path))
                    throw new ValidationException("path", "Invalid path format");
                
                if (!InputValidator.IsValidKey(key))
                    throw new ValidationException("key", "Invalid key format");

                var client = await GetAuthenticatedClientAsync();
                
                var secret = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                    path: path,
                    mountPoint: "secret"
                );

                if (secret?.Data?.Data != null && secret.Data.Data.ContainsKey(key))
                {
                    var secretValue = secret.Data.Data[key]?.ToString();
                    _logger.LogInformation("Secret read successfully from {Path}", path);
                    return secretValue;
                }

                _logger.LogWarning("Secret not found at {Path} with key {Key}", path, key);
                return null;
            }
            catch (Exception ex) when (!(ex is ValidationException || ex is AuthenticationException))
            {
                _logger.LogError(ex, "Failed to read secret from {Path}", path);
                throw new SecretOperationException("Read", path, $"Read operation failed: {ex.Message}", ex);
            }
        }

        public async Task<SealStatus> GetSealStatusAsync()
        {
            try
            {
                var client = VaultClientFactory.CreateUnauthenticatedClient(_config);
                return await client.V1.System.GetSealStatusAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get seal status");
                throw new VaultOperationException("GetSealStatus", $"Failed to get seal status: {ex.Message}", ex);
            }
        }

        public async Task<bool> GetInitializationStatusAsync()
        {
            try
            {
                var client = VaultClientFactory.CreateUnauthenticatedClient(_config);
                return await client.V1.System.GetInitStatusAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get initialization status");
                throw new VaultOperationException("GetInitStatus", $"Failed to get initialization status: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var client = await GetAuthenticatedClientAsync();
                var tokenInfo = await client.V1.Auth.Token.LookupSelfAsync();
                return tokenInfo?.Data != null;
            }
            catch
            {
                return false;
            }
        }

        private async Task<IVaultClient> GetAuthenticatedClientAsync()
        {
            if (_vaultClient != null)
            {
                // Verify current client is still valid
                try
                {
                    await _vaultClient.V1.Auth.Token.LookupSelfAsync();
                    return _vaultClient;
                }
                catch
                {
                    _vaultClient = null;
                }
            }

            // Try to load token from file
            var token = SecurityHelper.LoadSecureToken(_config.TokenFile);
            if (string.IsNullOrEmpty(token))
            {
                throw new AuthenticationException("No valid authentication token found. Please login first.");
            }

            _vaultClient = VaultClientFactory.CreateClient(_config, token);
            
            // Verify the loaded token
            try
            {
                await _vaultClient.V1.Auth.Token.LookupSelfAsync();
                return _vaultClient;
            }
            catch
            {
                _vaultClient = null;
                throw new AuthenticationException("Stored token is invalid. Please login again.");
            }
        }

        private async Task SaveInitializationDataAsync(InitializationData initData)
        {
            try
            {
                var json = JsonSerializer.Serialize(initData, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                await File.WriteAllTextAsync(_config.InitFile, json);
                
                // Set restrictive permissions on Unix systems
                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(_config.InitFile, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                }

                _logger.LogInformation("Initialization data saved to {File}", _config.InitFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save initialization data");
                throw new InitializationException($"Failed to save initialization data: {ex.Message}", ex);
            }
        }
    }
}
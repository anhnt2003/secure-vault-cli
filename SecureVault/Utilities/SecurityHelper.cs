using System.Text;

namespace SecureVault.Utilities
{
    public static class SecurityHelper
    {
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("VaultCliEntropy");

        public static void SaveSecureToken(string token, string filePath)
        {
            try
            {
                var tokenBytes = Encoding.UTF8.GetBytes(token);
                byte[] encryptedBytes;

                if (OperatingSystem.IsWindows())
                {
                    // For Windows, we'll use basic file protection for now
                    // In a production environment, consider using Windows DPAPI
                    encryptedBytes = tokenBytes;
                }
                else
                {
                    // For non-Windows platforms, use basic file protection
                    encryptedBytes = tokenBytes;
                }

                File.WriteAllBytes(filePath, encryptedBytes);
                
                // Set file permissions to be readable only by current user
                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(filePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save secure token: {ex.Message}", ex);
            }
        }

        public static string? LoadSecureToken(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                var encryptedBytes = File.ReadAllBytes(filePath);
                byte[] tokenBytes;

                if (OperatingSystem.IsWindows())
                {
                    // For Windows, we're using basic file storage for now
                    tokenBytes = encryptedBytes;
                }
                else
                {
                    // For non-Windows platforms, data is not encrypted
                    tokenBytes = encryptedBytes;
                }

                return Encoding.UTF8.GetString(tokenBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load secure token: {ex.Message}", ex);
            }
        }

        public static void ClearMemory(string sensitiveData)
        {
            // This is a best-effort approach to clear sensitive data from memory
            // Note: In .NET, strings are immutable and this won't actually clear the original string
            // For production use, consider using SecureString or similar approaches
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public static string GenerateSecureFileName(string baseName)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var randomSuffix = Path.GetRandomFileName().Replace(".", "")[..6];
            return $"{baseName}_{timestamp}_{randomSuffix}";
        }
    }
}
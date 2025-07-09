namespace SecureVault.Utilities
{
    public static class InputValidator
    {
        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            // Check for path traversal attempts
            if (path.Contains(".."))
                return false;

            // Check for invalid path characters
            if (Path.GetInvalidPathChars().Any(path.Contains))
                return false;

            return true;
        }

        public static bool IsValidKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            // Allow only alphanumeric characters, underscores, and hyphens
            return key.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
        }

        public static bool IsValidEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return false;

            return Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsValidToken(string token)
        {
            return !string.IsNullOrWhiteSpace(token) && token.Length >= 10;
        }
    }
}
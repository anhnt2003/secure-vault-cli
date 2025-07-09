namespace SecureVault.UnitTests.Services;

public class ConfigurationServiceTests
{
    private readonly Mock<ILogger<ConfigurationService>> _mockLogger;
    private readonly ConfigurationService _configurationService;

    public ConfigurationServiceTests()
    {
        _mockLogger = new Mock<ILogger<ConfigurationService>>();
        _configurationService = new ConfigurationService(_mockLogger.Object);
    }

    [Fact]
    public void LoadConfiguration_WithDefaultValues_ReturnsDefaultConfiguration()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var result = _configurationService.LoadConfiguration(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("http://127.0.0.1:8200", result.VaultAddress);
        Assert.False(result.SkipTlsVerification);
        Assert.Null(result.Token);
    }

    [Fact]
    public void LoadConfiguration_WithEnvironmentVariables_ReturnsConfigurationFromEnvironment()
    {
        // Arrange
        var args = Array.Empty<string>();
        Environment.SetEnvironmentVariable("VAULT_ADDR", "https://vault.example.com:8200");
        Environment.SetEnvironmentVariable("VAULT_TOKEN", "test-token");
        Environment.SetEnvironmentVariable("VAULT_SKIP_VERIFY", "true");

        try
        {
            // Act
            var result = _configurationService.LoadConfiguration(args);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://vault.example.com:8200", result.VaultAddress);
            Assert.Equal("test-token", result.Token);
            Assert.True(result.SkipTlsVerification);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("VAULT_ADDR", null);
            Environment.SetEnvironmentVariable("VAULT_TOKEN", null);
            Environment.SetEnvironmentVariable("VAULT_SKIP_VERIFY", null);
        }
    }

    [Fact]
    public void LoadConfiguration_WithCommandLineArguments_ReturnsConfigurationFromArgs()
    {
        // Arrange
        var args = new[] { "--vault-addr", "https://cli.vault.com:8200", "--skip-tls-verify", "true" };

        // Act
        var result = _configurationService.LoadConfiguration(args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://cli.vault.com:8200", result.VaultAddress);
        Assert.True(result.SkipTlsVerification);
    }

    [Fact]
    public void LoadConfiguration_WithMultipleSources_CommandLineOverridesEnvironment()
    {
        // Arrange
        var args = new[] { "--vault-addr", "https://cli.vault.com:8200" };
        Environment.SetEnvironmentVariable("VAULT_ADDR", "https://env.vault.com:8200");
        Environment.SetEnvironmentVariable("VAULT_TOKEN", "env-token");

        try
        {
            // Act
            var result = _configurationService.LoadConfiguration(args);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://cli.vault.com:8200", result.VaultAddress); // CLI overrides env
            Assert.Equal("env-token", result.Token); // Env value used when not in CLI
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("VAULT_ADDR", null);
            Environment.SetEnvironmentVariable("VAULT_TOKEN", null);
        }
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("yes", true)]
    [InlineData("no", false)]
    public void LoadConfiguration_WithSkipTlsVerificationValues_ParsesCorrectly(string value, bool expected)
    {
        // Arrange
        Environment.SetEnvironmentVariable("VAULT_SKIP_VERIFY", value);

        try
        {
            // Act
            var result = _configurationService.LoadConfiguration(Array.Empty<string>());

            // Assert
            Assert.Equal(expected, result.SkipTlsVerification);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("VAULT_SKIP_VERIFY", null);
        }
    }

    [Fact]
    public void LoadConfiguration_WithInvalidSkipTlsValue_DefaultsToFalse()
    {
        // Arrange
        Environment.SetEnvironmentVariable("VAULT_SKIP_VERIFY", "invalid");

        try
        {
            // Act
            var result = _configurationService.LoadConfiguration(Array.Empty<string>());

            // Assert
            Assert.False(result.SkipTlsVerification);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("VAULT_SKIP_VERIFY", null);
        }
    }
}
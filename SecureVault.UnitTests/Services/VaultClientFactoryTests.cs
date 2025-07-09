namespace SecureVault.UnitTests.Services;

public class VaultClientFactoryTests
{
    private readonly Mock<ILogger<VaultClientFactory>> _mockLogger;
    private readonly VaultClientFactory _factory;

    public VaultClientFactoryTests()
    {
        _mockLogger = new Mock<ILogger<VaultClientFactory>>();
        _factory = new VaultClientFactory(_mockLogger.Object);
    }

    [Fact]
    public void CreateClient_WithValidConfiguration_ReturnsVaultClient()
    {
        // Arrange
        var configuration = new VaultConfiguration
        {
            VaultAddress = "http://127.0.0.1:8200",
            Token = "test-token",
            SkipTlsVerification = false
        };

        // Act
        var result = _factory.CreateClient(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IVaultClient>(result);
    }

    [Fact]
    public void CreateClient_WithHttpsAndSkipTlsVerification_ReturnsVaultClient()
    {
        // Arrange
        var configuration = new VaultConfiguration
        {
            VaultAddress = "https://vault.example.com:8200",
            Token = "test-token",
            SkipTlsVerification = true
        };

        // Act
        var result = _factory.CreateClient(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IVaultClient>(result);
    }

    [Fact]
    public void CreateClient_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _factory.CreateClient(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateClient_WithInvalidVaultAddress_ThrowsArgumentException(string? vaultAddress)
    {
        // Arrange
        var configuration = new VaultConfiguration
        {
            VaultAddress = vaultAddress!,
            Token = "test-token",
            SkipTlsVerification = false
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _factory.CreateClient(configuration));
    }

    [Fact]
    public void CreateClient_WithInvalidUri_ThrowsUriFormatException()
    {
        // Arrange
        var configuration = new VaultConfiguration
        {
            VaultAddress = "invalid-uri",
            Token = "test-token",
            SkipTlsVerification = false
        };

        // Act & Assert
        Assert.Throws<UriFormatException>(() => _factory.CreateClient(configuration));
    }

    [Fact]
    public void CreateClient_WithoutToken_ReturnsVaultClientWithoutAuth()
    {
        // Arrange
        var configuration = new VaultConfiguration
        {
            VaultAddress = "http://127.0.0.1:8200",
            Token = null,
            SkipTlsVerification = false
        };

        // Act
        var result = _factory.CreateClient(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IVaultClient>(result);
    }

    [Theory]
    [InlineData("http://127.0.0.1:8200")]
    [InlineData("https://vault.example.com:8200")]
    [InlineData("http://localhost:8200")]
    public void CreateClient_WithVariousValidAddresses_ReturnsVaultClient(string vaultAddress)
    {
        // Arrange
        var configuration = new VaultConfiguration
        {
            VaultAddress = vaultAddress,
            Token = "test-token",
            SkipTlsVerification = false
        };

        // Act
        var result = _factory.CreateClient(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IVaultClient>(result);
    }
}
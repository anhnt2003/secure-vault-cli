namespace SecureVault.UnitTests.Services;

public class VaultServiceTests
{
    private readonly Mock<IVaultClient> _mockVaultClient;
    private readonly Mock<ILogger<VaultService>> _mockLogger;
    private readonly VaultService _vaultService;

    public VaultServiceTests()
    {
        _mockVaultClient = new Mock<IVaultClient>();
        _mockLogger = new Mock<ILogger<VaultService>>();
        _vaultService = new VaultService(_mockVaultClient.Object, _mockLogger.Object);
    }

    #region InitializeAsync Tests

    [Fact]
    public async Task InitializeAsync_WithValidParameters_ReturnsInitializationData()
    {
        // Arrange
        var keyShares = 5;
        var keyThreshold = 3;
        var initResponse = new InitializeResponse
        {
            Keys = new[] { "key1", "key2", "key3", "key4", "key5" },
            RootToken = "root-token"
        };

        _mockVaultClient.Setup(x => x.V1.System.InitializeAsync(It.IsAny<InitializeRequest>()))
            .ReturnsAsync(initResponse);

        // Act
        var result = await _vaultService.InitializeAsync(keyShares, keyThreshold);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Keys.Count);
        Assert.Equal("root-token", result.RootToken);
        Assert.Equal(keyShares, result.KeyShares);
        Assert.Equal(keyThreshold, result.KeyThreshold);
    }

    [Fact]
    public async Task InitializeAsync_WhenVaultClientThrows_ThrowsVaultOperationException()
    {
        // Arrange
        _mockVaultClient.Setup(x => x.V1.System.InitializeAsync(It.IsAny<InitializeRequest>()))
            .ThrowsAsync(new Exception("Vault error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<VaultOperationException>(
            () => _vaultService.InitializeAsync(5, 3));
        
        Assert.Contains("Failed to initialize Vault", exception.Message);
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(-1, 3)]
    [InlineData(5, 0)]
    [InlineData(5, -1)]
    [InlineData(5, 6)] // threshold > shares
    public async Task InitializeAsync_WithInvalidParameters_ThrowsArgumentException(int keyShares, int keyThreshold)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _vaultService.InitializeAsync(keyShares, keyThreshold));
    }

    #endregion

    #region UnsealAsync Tests

    [Fact]
    public async Task UnsealAsync_WithValidKey_ReturnsUnsealResponse()
    {
        // Arrange
        var key = "test-key";
        var unsealResponse = new UnsealResponse
        {
            Sealed = false,
            Progress = 3,
            Threshold = 3
        };

        _mockVaultClient.Setup(x => x.V1.System.UnsealAsync(key))
            .ReturnsAsync(unsealResponse);

        // Act
        var result = await _vaultService.UnsealAsync(key);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Sealed);
        Assert.Equal(3, result.Progress);
        Assert.Equal(3, result.Threshold);
    }

    [Fact]
    public async Task UnsealAsync_WhenVaultClientThrows_ThrowsVaultOperationException()
    {
        // Arrange
        _mockVaultClient.Setup(x => x.V1.System.UnsealAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Vault error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<VaultOperationException>(
            () => _vaultService.UnsealAsync("test-key"));
        
        Assert.Contains("Failed to unseal Vault", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UnsealAsync_WithInvalidKey_ThrowsArgumentException(string? key)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _vaultService.UnsealAsync(key!));
    }

    #endregion

    #region GetSealStatusAsync Tests

    [Fact]
    public async Task GetSealStatusAsync_ReturnsCorrectStatus()
    {
        // Arrange
        var sealStatusResponse = new SealStatusResponse
        {
            Sealed = true,
            Progress = 1,
            Threshold = 3,
            Total = 5
        };

        _mockVaultClient.Setup(x => x.V1.System.GetSealStatusAsync())
            .ReturnsAsync(sealStatusResponse);

        // Act
        var result = await _vaultService.GetSealStatusAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Sealed);
        Assert.Equal(1, result.Progress);
        Assert.Equal(3, result.Threshold);
        Assert.Equal(5, result.Total);
    }

    [Fact]
    public async Task GetSealStatusAsync_WhenVaultClientThrows_ThrowsVaultOperationException()
    {
        // Arrange
        _mockVaultClient.Setup(x => x.V1.System.GetSealStatusAsync())
            .ThrowsAsync(new Exception("Vault error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<VaultOperationException>(
            () => _vaultService.GetSealStatusAsync());
        
        Assert.Contains("Failed to get seal status", exception.Message);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var token = "test-token";
        var tokenInfo = new TokenInfo
        {
            Id = token,
            Policies = new[] { "default" }
        };

        _mockVaultClient.Setup(x => x.V1.Auth.Token.LookupSelfAsync())
            .ReturnsAsync(new Secret<TokenInfo> { Data = tokenInfo });

        // Act
        var result = await _vaultService.LoginAsync(token);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        _mockVaultClient.Setup(x => x.V1.Auth.Token.LookupSelfAsync())
            .ThrowsAsync(new Exception("Invalid token"));

        // Act
        var result = await _vaultService.LoginAsync("invalid-token");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LoginAsync_WithInvalidToken_ThrowsArgumentException(string? token)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _vaultService.LoginAsync(token!));
    }

    #endregion

    #region WriteSecretAsync Tests

    [Fact]
    public async Task WriteSecretAsync_WithValidParameters_WritesSecret()
    {
        // Arrange
        var path = "myapp/config";
        var key = "password";
        var value = "secret123";

        _mockVaultClient.Setup(x => x.V1.Secrets.KeyValue.V2.WriteSecretAsync(
            It.IsAny<string>(), It.IsAny<IDictionary<string, object>>(), null, null))
            .Returns(Task.CompletedTask);

        // Act
        await _vaultService.WriteSecretAsync(path, key, value);

        // Assert
        _mockVaultClient.Verify(x => x.V1.Secrets.KeyValue.V2.WriteSecretAsync(
            path, It.Is<IDictionary<string, object>>(d => d[key].ToString() == value), null, null), 
            Times.Once);
    }

    [Fact]
    public async Task WriteSecretAsync_WhenVaultClientThrows_ThrowsVaultOperationException()
    {
        // Arrange
        _mockVaultClient.Setup(x => x.V1.Secrets.KeyValue.V2.WriteSecretAsync(
            It.IsAny<string>(), It.IsAny<IDictionary<string, object>>(), null, null))
            .ThrowsAsync(new Exception("Vault error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<VaultOperationException>(
            () => _vaultService.WriteSecretAsync("path", "key", "value"));
        
        Assert.Contains("Failed to write secret", exception.Message);
    }

    [Theory]
    [InlineData(null, "key", "value")]
    [InlineData("", "key", "value")]
    [InlineData("path", null, "value")]
    [InlineData("path", "", "value")]
    [InlineData("path", "key", null)]
    public async Task WriteSecretAsync_WithInvalidParameters_ThrowsArgumentException(
        string? path, string? key, string? value)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _vaultService.WriteSecretAsync(path!, key!, value!));
    }

    #endregion

    #region ReadSecretAsync Tests

    [Fact]
    public async Task ReadSecretAsync_WithValidParameters_ReturnsSecret()
    {
        // Arrange
        var path = "myapp/config";
        var key = "password";
        var expectedValue = "secret123";
        
        var secretData = new Dictionary<string, object> { { key, expectedValue } };
        var secret = new Secret<SecretData>
        {
            Data = new SecretData { Data = secretData }
        };

        _mockVaultClient.Setup(x => x.V1.Secrets.KeyValue.V2.ReadSecretAsync(path, null, null))
            .ReturnsAsync(secret);

        // Act
        var result = await _vaultService.ReadSecretAsync(path, key);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task ReadSecretAsync_WhenSecretNotFound_ReturnsNull()
    {
        // Arrange
        _mockVaultClient.Setup(x => x.V1.Secrets.KeyValue.V2.ReadSecretAsync(It.IsAny<string>(), null, null))
            .ReturnsAsync((Secret<SecretData>?)null);

        // Act
        var result = await _vaultService.ReadSecretAsync("nonexistent", "key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ReadSecretAsync_WhenKeyNotFound_ReturnsNull()
    {
        // Arrange
        var secretData = new Dictionary<string, object> { { "other-key", "value" } };
        var secret = new Secret<SecretData>
        {
            Data = new SecretData { Data = secretData }
        };

        _mockVaultClient.Setup(x => x.V1.Secrets.KeyValue.V2.ReadSecretAsync(It.IsAny<string>(), null, null))
            .ReturnsAsync(secret);

        // Act
        var result = await _vaultService.ReadSecretAsync("path", "nonexistent-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ReadSecretAsync_WhenVaultClientThrows_ThrowsVaultOperationException()
    {
        // Arrange
        _mockVaultClient.Setup(x => x.V1.Secrets.KeyValue.V2.ReadSecretAsync(It.IsAny<string>(), null, null))
            .ThrowsAsync(new Exception("Vault error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<VaultOperationException>(
            () => _vaultService.ReadSecretAsync("path", "key"));
        
        Assert.Contains("Failed to read secret", exception.Message);
    }

    [Theory]
    [InlineData(null, "key")]
    [InlineData("", "key")]
    [InlineData("path", null)]
    [InlineData("path", "")]
    public async Task ReadSecretAsync_WithInvalidParameters_ThrowsArgumentException(
        string? path, string? key)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _vaultService.ReadSecretAsync(path!, key!));
    }

    #endregion
}
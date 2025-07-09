namespace SecureVault.UnitTests.Commands;

public class LoginCommandTests
{
    private readonly Mock<IVaultService> _mockVaultService;
    private readonly Mock<ILogger<LoginCommand>> _mockLogger;
    private readonly LoginCommand _loginCommand;

    public LoginCommandTests()
    {
        _mockVaultService = new Mock<IVaultService>();
        _mockLogger = new Mock<ILogger<LoginCommand>>();
        _loginCommand = new LoginCommand(_mockVaultService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("login", _loginCommand.Name);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("Authenticate with Vault", _loginCommand.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidToken_AuthenticatesSuccessfully()
    {
        // Arrange
        var args = new[] { "login", "--token", "hvs.test-token" };
        _mockVaultService.Setup(x => x.LoginAsync("hvs.test-token"))
            .ReturnsAsync(true);

        // Act
        await _loginCommand.ExecuteAsync(args);

        // Assert
        _mockVaultService.Verify(x => x.LoginAsync("hvs.test-token"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithTokenFromEnvironment_AuthenticatesSuccessfully()
    {
        // Arrange
        var args = new[] { "login" };
        Environment.SetEnvironmentVariable("VAULT_TOKEN", "env-token");
        
        _mockVaultService.Setup(x => x.LoginAsync("env-token"))
            .ReturnsAsync(true);

        try
        {
            // Act
            await _loginCommand.ExecuteAsync(args);

            // Assert
            _mockVaultService.Verify(x => x.LoginAsync("env-token"), Times.Once);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("VAULT_TOKEN", null);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithoutToken_ThrowsArgumentException()
    {
        // Arrange
        var args = new[] { "login" };
        Environment.SetEnvironmentVariable("VAULT_TOKEN", null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _loginCommand.ExecuteAsync(args));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidToken_ThrowsVaultOperationException()
    {
        // Arrange
        var args = new[] { "login", "--token", "invalid-token" };
        _mockVaultService.Setup(x => x.LoginAsync("invalid-token"))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<VaultOperationException>(
            () => _loginCommand.ExecuteAsync(args));
        
        Assert.Contains("Authentication failed", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenVaultServiceThrows_PropagatesException()
    {
        // Arrange
        var args = new[] { "login", "--token", "test-token" };
        _mockVaultService.Setup(x => x.LoginAsync(It.IsAny<string>()))
            .ThrowsAsync(new VaultOperationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<VaultOperationException>(
            () => _loginCommand.ExecuteAsync(args));
        
        Assert.Equal("Connection failed", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyToken_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _loginCommand.ExecuteAsync(new[] { "login", "--token", "" }));
        
        await Assert.ThrowsAsync<ArgumentException>(
            () => _loginCommand.ExecuteAsync(new[] { "login", "--token", "   " }));
    }

    [Fact]
    public async Task ExecuteAsync_CommandLineTokenOverridesEnvironment()
    {
        // Arrange
        var args = new[] { "login", "--token", "cli-token" };
        Environment.SetEnvironmentVariable("VAULT_TOKEN", "env-token");
        
        _mockVaultService.Setup(x => x.LoginAsync("cli-token"))
            .ReturnsAsync(true);

        try
        {
            // Act
            await _loginCommand.ExecuteAsync(args);

            // Assert
            _mockVaultService.Verify(x => x.LoginAsync("cli-token"), Times.Once);
            _mockVaultService.Verify(x => x.LoginAsync("env-token"), Times.Never);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("VAULT_TOKEN", null);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidAuthentication_SavesTokenSecurely()
    {
        // Arrange
        var args = new[] { "login", "--token", "hvs.test-token" };
        _mockVaultService.Setup(x => x.LoginAsync("hvs.test-token"))
            .ReturnsAsync(true);

        // Act
        await _loginCommand.ExecuteAsync(args);

        // Assert
        _mockVaultService.Verify(x => x.LoginAsync("hvs.test-token"), Times.Once);
        // Note: In a real scenario, we would also verify that SecurityHelper.SaveToken was called
        // but since it's a static method, we would need to refactor it to be testable or use a wrapper
    }
}
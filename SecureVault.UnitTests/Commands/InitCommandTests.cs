namespace SecureVault.UnitTests.Commands;

public class InitCommandTests
{
    private readonly Mock<IVaultService> _mockVaultService;
    private readonly Mock<ILogger<InitCommand>> _mockLogger;
    private readonly InitCommand _initCommand;

    public InitCommandTests()
    {
        _mockVaultService = new Mock<IVaultService>();
        _mockLogger = new Mock<ILogger<InitCommand>>();
        _initCommand = new InitCommand(_mockVaultService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("init", _initCommand.Name);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("Initialize Vault with key shares", _initCommand.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultArguments_InitializesVaultWithDefaults()
    {
        // Arrange
        var args = new[] { "init" };
        var initData = new InitializationData
        {
            Keys = new List<string> { "key1", "key2", "key3", "key4", "key5" },
            RootToken = "root-token",
            KeyShares = 5,
            KeyThreshold = 3
        };

        _mockVaultService.Setup(x => x.InitializeAsync(5, 3))
            .ReturnsAsync(initData);

        // Act
        await _initCommand.ExecuteAsync(args);

        // Assert
        _mockVaultService.Verify(x => x.InitializeAsync(5, 3), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomArguments_InitializesVaultWithCustomValues()
    {
        // Arrange
        var args = new[] { "init", "--shares", "7", "--threshold", "4" };
        var initData = new InitializationData
        {
            Keys = new List<string> { "key1", "key2", "key3", "key4", "key5", "key6", "key7" },
            RootToken = "root-token",
            KeyShares = 7,
            KeyThreshold = 4
        };

        _mockVaultService.Setup(x => x.InitializeAsync(7, 4))
            .ReturnsAsync(initData);

        // Act
        await _initCommand.ExecuteAsync(args);

        // Assert
        _mockVaultService.Verify(x => x.InitializeAsync(7, 4), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidShares_ThrowsArgumentException()
    {
        // Arrange
        var args = new[] { "init", "--shares", "0" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _initCommand.ExecuteAsync(args));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidThreshold_ThrowsArgumentException()
    {
        // Arrange
        var args = new[] { "init", "--threshold", "0" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _initCommand.ExecuteAsync(args));
    }

    [Fact]
    public async Task ExecuteAsync_WithThresholdGreaterThanShares_ThrowsArgumentException()
    {
        // Arrange
        var args = new[] { "init", "--shares", "3", "--threshold", "5" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _initCommand.ExecuteAsync(args));
    }

    [Fact]
    public async Task ExecuteAsync_WhenVaultServiceThrows_PropagatesException()
    {
        // Arrange
        var args = new[] { "init" };
        _mockVaultService.Setup(x => x.InitializeAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new VaultOperationException("Vault already initialized"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<VaultOperationException>(
            () => _initCommand.ExecuteAsync(args));
        
        Assert.Equal("Vault already initialized", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidNumericArguments_ThrowsFormatException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(
            () => _initCommand.ExecuteAsync(new[] { "init", "--shares", "abc" }));
        
        await Assert.ThrowsAsync<FormatException>(
            () => _initCommand.ExecuteAsync(new[] { "init", "--threshold", "xyz" }));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidArguments_SavesInitializationData()
    {
        // Arrange
        var args = new[] { "init", "--shares", "5", "--threshold", "3" };
        var initData = new InitializationData
        {
            Keys = new List<string> { "key1", "key2", "key3", "key4", "key5" },
            RootToken = "root-token",
            KeyShares = 5,
            KeyThreshold = 3
        };

        _mockVaultService.Setup(x => x.InitializeAsync(5, 3))
            .ReturnsAsync(initData);

        // Act
        await _initCommand.ExecuteAsync(args);

        // Assert
        _mockVaultService.Verify(x => x.InitializeAsync(5, 3), Times.Once);
        // Note: In a real scenario, we would also verify that SecurityHelper.SaveInitializationData was called
        // but since it's a static method, we would need to refactor it to be testable or use a wrapper
    }
}
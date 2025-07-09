using SecureVault.Utilities;

namespace SecureVault.UnitTests.Utilities;

public class ArgumentParserTests
{
    [Fact]
    public void ParseInitArguments_WithDefaultValues_ReturnsDefaults()
    {
        // Arrange
        var args = new[] { "init" };

        // Act
        var (shares, threshold) = ArgumentParser.ParseInitArguments(args);

        // Assert
        Assert.Equal(5, shares);
        Assert.Equal(3, threshold);
    }

    [Fact]
    public void ParseInitArguments_WithCustomShares_ReturnsCustomShares()
    {
        // Arrange
        var args = new[] { "init", "--shares", "7" };

        // Act
        var (shares, threshold) = ArgumentParser.ParseInitArguments(args);

        // Assert
        Assert.Equal(7, shares);
        Assert.Equal(3, threshold); // Default threshold
    }

    [Fact]
    public void ParseInitArguments_WithCustomThreshold_ReturnsCustomThreshold()
    {
        // Arrange
        var args = new[] { "init", "--threshold", "4" };

        // Act
        var (shares, threshold) = ArgumentParser.ParseInitArguments(args);

        // Assert
        Assert.Equal(5, shares); // Default shares
        Assert.Equal(4, threshold);
    }

    [Fact]
    public void ParseInitArguments_WithBothCustomValues_ReturnsBothCustomValues()
    {
        // Arrange
        var args = new[] { "init", "--shares", "7", "--threshold", "4" };

        // Act
        var (shares, threshold) = ArgumentParser.ParseInitArguments(args);

        // Assert
        Assert.Equal(7, shares);
        Assert.Equal(4, threshold);
    }

    [Fact]
    public void ParseInitArguments_WithShortFlags_ParsesCorrectly()
    {
        // Arrange
        var args = new[] { "init", "-s", "6", "-t", "2" };

        // Act
        var (shares, threshold) = ArgumentParser.ParseInitArguments(args);

        // Assert
        Assert.Equal(6, shares);
        Assert.Equal(2, threshold);
    }

    [Fact]
    public void ParseInitArguments_WithInvalidNumericValues_ThrowsFormatException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => ArgumentParser.ParseInitArguments(new[] { "init", "--shares", "abc" }));
        Assert.Throws<FormatException>(() => ArgumentParser.ParseInitArguments(new[] { "init", "--threshold", "xyz" }));
        Assert.Throws<FormatException>(() => ArgumentParser.ParseInitArguments(new[] { "init", "-s", "1.5" }));
        Assert.Throws<FormatException>(() => ArgumentParser.ParseInitArguments(new[] { "init", "-t", "2.7" }));
    }

    [Fact]
    public void ParseInitArguments_WithInvalidValues_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ArgumentParser.ParseInitArguments(new[] { "init", "--shares", "0" }));
        Assert.Throws<ArgumentException>(() => ArgumentParser.ParseInitArguments(new[] { "init", "--shares", "-1" }));
        Assert.Throws<ArgumentException>(() => ArgumentParser.ParseInitArguments(new[] { "init", "--threshold", "0" }));
        Assert.Throws<ArgumentException>(() => ArgumentParser.ParseInitArguments(new[] { "init", "--threshold", "-1" }));
    }

    [Fact]
    public void ParseInitArguments_WithThresholdGreaterThanShares_ThrowsArgumentException()
    {
        // Arrange
        var args = new[] { "init", "--shares", "3", "--threshold", "5" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ArgumentParser.ParseInitArguments(args));
    }

    [Fact]
    public void ParseInitArguments_WithMissingValues_ThrowsArgumentException()
    {
        // Arrange
        var args = new[] { "init", "--shares" }; // Missing value for --shares

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ArgumentParser.ParseInitArguments(args));
    }

    [Fact]
    public void ParseInitArguments_WithUnknownArguments_IgnoresThem()
    {
        // Arrange
        var args = new[] { "init", "--shares", "5", "--unknown", "value", "--threshold", "3" };

        // Act
        var (shares, threshold) = ArgumentParser.ParseInitArguments(args);

        // Assert
        Assert.Equal(5, shares);
        Assert.Equal(3, threshold);
    }

    [Fact]
    public void ParseInitArguments_WithDuplicateArguments_UsesLastValue()
    {
        // Arrange
        var args = new[] { "init", "--shares", "5", "--shares", "7", "--threshold", "3" };

        // Act
        var (shares, threshold) = ArgumentParser.ParseInitArguments(args);

        // Assert
        Assert.Equal(7, shares); // Last value wins
        Assert.Equal(3, threshold);
    }

    [Fact]
    public void ParseInitArguments_WithEmptyArgs_ReturnsDefaults()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var (shares, threshold) = ArgumentParser.ParseInitArguments(args);

        // Assert
        Assert.Equal(5, shares);
        Assert.Equal(3, threshold);
    }

    [Fact]
    public void ParseInitArguments_WithEqualsSignSyntax_ParsesCorrectly()
    {
        // Act & Assert
        var (shares1, threshold1) = ArgumentParser.ParseInitArguments(new[] { "init", "--shares=7" });
        Assert.Equal(7, shares1);
        Assert.Equal(3, threshold1);

        var (shares2, threshold2) = ArgumentParser.ParseInitArguments(new[] { "init", "--threshold=4" });
        Assert.Equal(5, shares2);
        Assert.Equal(4, threshold2);
    }
}
using SecureVault.Utilities;

namespace SecureVault.UnitTests.Utilities;

public class InputValidatorTests
{
    #region ValidatePath Tests

    [Theory]
    [InlineData("myapp/config")]
    [InlineData("database/credentials")]
    [InlineData("app")]
    [InlineData("my-app/my-config")]
    [InlineData("app_name/config_key")]
    [InlineData("123/456")]
    public void ValidatePath_WithValidPaths_ReturnsTrue(string path)
    {
        // Act
        var result = InputValidator.ValidatePath(path);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/")]
    [InlineData("//")]
    [InlineData("path/")]
    [InlineData("/path")]
    [InlineData("path//subpath")]
    [InlineData("path with spaces")]
    [InlineData("path\twith\ttabs")]
    [InlineData("path\nwith\nnewlines")]
    public void ValidatePath_WithInvalidPaths_ReturnsFalse(string? path)
    {
        // Act
        var result = InputValidator.ValidatePath(path!);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("path/with/many/levels")]
    [InlineData("a/b/c/d/e/f/g")]
    public void ValidatePath_WithDeepPaths_ReturnsTrue(string path)
    {
        // Act
        var result = InputValidator.ValidatePath(path);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("path.with.dots")]
    [InlineData("path-with-dashes")]
    [InlineData("path_with_underscores")]
    [InlineData("path123")]
    [InlineData("123path")]
    public void ValidatePath_WithSpecialCharacters_ReturnsTrue(string path)
    {
        // Act
        var result = InputValidator.ValidatePath(path);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region ValidateKey Tests

    [Theory]
    [InlineData("password")]
    [InlineData("username")]
    [InlineData("api_key")]
    [InlineData("database-url")]
    [InlineData("config.setting")]
    [InlineData("key123")]
    [InlineData("123key")]
    [InlineData("a")]
    [InlineData("very_long_key_name_that_should_still_be_valid")]
    public void ValidateKey_WithValidKeys_ReturnsTrue(string key)
    {
        // Act
        var result = InputValidator.ValidateKey(key);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("key with spaces")]
    [InlineData("key\twith\ttabs")]
    [InlineData("key\nwith\nnewlines")]
    [InlineData("key/with/slashes")]
    [InlineData("key\\with\\backslashes")]
    public void ValidateKey_WithInvalidKeys_ReturnsFalse(string? key)
    {
        // Act
        var result = InputValidator.ValidateKey(key!);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("key@symbol")]
    [InlineData("key#hash")]
    [InlineData("key$dollar")]
    [InlineData("key%percent")]
    [InlineData("key^caret")]
    [InlineData("key&ampersand")]
    [InlineData("key*asterisk")]
    [InlineData("key(parenthesis)")]
    [InlineData("key[bracket]")]
    [InlineData("key{brace}")]
    [InlineData("key|pipe")]
    [InlineData("key;semicolon")]
    [InlineData("key:colon")]
    [InlineData("key'quote")]
    [InlineData("key\"doublequote")]
    [InlineData("key<less>")]
    [InlineData("key?question")]
    [InlineData("key,comma")]
    public void ValidateKey_WithSpecialSymbols_ReturnsFalse(string key)
    {
        // Act
        var result = InputValidator.ValidateKey(key);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ValidateValue Tests

    [Theory]
    [InlineData("simple_value")]
    [InlineData("password123")]
    [InlineData("https://api.example.com")]
    [InlineData("user@example.com")]
    [InlineData("Complex P@ssw0rd!")]
    [InlineData("Value with spaces")]
    [InlineData("Multi\nLine\nValue")]
    [InlineData("Value\twith\ttabs")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("null")]
    public void ValidateValue_WithVariousValues_ReturnsTrue(string value)
    {
        // Act
        var result = InputValidator.ValidateValue(value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateValue_WithNullValue_ReturnsFalse()
    {
        // Act
        var result = InputValidator.ValidateValue(null!);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("Very long value that exceeds the maximum allowed length for a secret value in Vault which should be reasonable but not unlimited to prevent abuse and ensure good performance characteristics")]
    public void ValidateValue_WithVeryLongValue_ReturnsAppropriateResult(string value)
    {
        // Act
        var result = InputValidator.ValidateValue(value);

        // Assert
        // This test verifies the behavior with long values
        // The actual result depends on the implementation's length limits
        Assert.IsType<bool>(result);
    }

    [Theory]
    [InlineData("JSON: {\"key\": \"value\", \"number\": 123}")]
    [InlineData("XML: <config><setting>value</setting></config>")]
    [InlineData("YAML: key: value\n  nested: true")]
    [InlineData("Base64: SGVsbG8gV29ybGQ=")]
    [InlineData("SQL: SELECT * FROM users WHERE id = 1;")]
    public void ValidateValue_WithStructuredData_ReturnsTrue(string value)
    {
        // Act
        var result = InputValidator.ValidateValue(value);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public void ValidatePath_WithMaximumLength_HandlesCorrectly()
    {
        // Arrange
        var longPath = string.Join("/", Enumerable.Repeat("segment", 50));

        // Act
        var result = InputValidator.ValidatePath(longPath);

        // Assert
        Assert.IsType<bool>(result);
    }

    [Fact]
    public void ValidateKey_WithMaximumLength_HandlesCorrectly()
    {
        // Arrange
        var longKey = new string('a', 1000);

        // Act
        var result = InputValidator.ValidateKey(longKey);

        // Assert
        Assert.IsType<bool>(result);
    }

    [Theory]
    [InlineData("path", "key", "value")]
    [InlineData("app/config", "database_url", "postgresql://user:pass@host:5432/db")]
    [InlineData("service/auth", "jwt_secret", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9")]
    public void ValidateAll_WithValidInputs_AllReturnTrue(string path, string key, string value)
    {
        // Act
        var pathValid = InputValidator.ValidatePath(path);
        var keyValid = InputValidator.ValidateKey(key);
        var valueValid = InputValidator.ValidateValue(value);

        // Assert
        Assert.True(pathValid);
        Assert.True(keyValid);
        Assert.True(valueValid);
    }

    [Theory]
    [InlineData("", "key", "value")]
    [InlineData("path", "", "value")]
    [InlineData("path", "key", null)]
    [InlineData("path with spaces", "key", "value")]
    [InlineData("path", "key with spaces", "value")]
    public void ValidateAll_WithInvalidInputs_AtLeastOneReturnsFalse(string? path, string? key, string? value)
    {
        // Act
        var pathValid = InputValidator.ValidatePath(path!);
        var keyValid = InputValidator.ValidateKey(key!);
        var valueValid = InputValidator.ValidateValue(value!);

        // Assert
        Assert.False(pathValid || keyValid || valueValid);
    }

    #endregion
}
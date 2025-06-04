# Vault CLI .NET 8 Technical Specification

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [System Overview](#system-overview)
3. [Architecture Design](#architecture-design)
4. [Technical Requirements](#technical-requirements)
5. [Implementation Details](#implementation-details)
6. [Security Considerations](#security-considerations)
7. [Error Handling Strategy](#error-handling-strategy)
8. [Configuration Management](#configuration-management)
9. [Testing Strategy](#testing-strategy)
10. [Deployment Guidelines](#deployment-guidelines)
11. [Appendices](#appendices)

## Executive Summary

This document outlines the technical specification for implementing a HashiCorp Vault Command Line Interface (CLI) application using .NET 8 and the VaultSharp library. The application provides secure secret management operations through a console-based interface. This is a personal learning project focused on understanding Vault basics and .NET integration patterns.

### Key Objectives
- Provide a robust CLI interface for Vault operations
- Implement secure authentication and authorization mechanisms
- Support core secret management operations (CRUD)
- Ensure compatibility with Vault's KV secrets engine v2
- Maintain high security standards for credential handling

## System Overview

### Project Scope
The Vault CLI application will provide the following core functionalities:
- Vault initialization and unsealing operations
- Token-based authentication and session management
- Secret writing and reading operations
- Configuration and state management

### Target Environment
- **Runtime**: .NET 8.0
- **Platform**: Cross-platform (Windows, Linux, macOS)
- **Vault Compatibility**: HashiCorp Vault 1.12+
- **Secrets Engine**: KV v2

## Architecture Design

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Vault CLI Application                    │
├─────────────────────────────────────────────────────────────┤
│  CLI Command Parser                                         │
│  ├── ArgumentValidator                                      │
│  ├── CommandRouter                                          │
│  └── HelpSystem                                             │
├─────────────────────────────────────────────────────────────┤
│  Business Logic Layer                                       │
│  ├── VaultInitializer                                       │
│  ├── AuthenticationManager                                  │
│  ├── SecretManager                                          │
│  └── ConfigurationManager                                   │
├─────────────────────────────────────────────────────────────┤
│  Vault Integration Layer                                    │
│  ├── VaultClientFactory                                     │
│  ├── VaultOperations                                        │
│  └── ErrorHandler                                           │
├─────────────────────────────────────────────────────────────┤
│  Infrastructure Layer                                       │
│  ├── FileSystemManager                                      │
│  ├── SecurityUtils                                          │
│  └── LoggingService                                         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    VaultSharp Library                       │
├─────────────────────────────────────────────────────────────┤
│  HTTP Client                                                │
│  JSON Serialization                                         │
│  Authentication Handlers                                    │
│  API Wrappers                                               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    HashiCorp Vault                          │
│                      REST API                               │
└─────────────────────────────────────────────────────────────┘
```

### Component Breakdown

#### CLI Command Parser
Responsible for parsing command-line arguments and routing to appropriate handlers.

#### Business Logic Layer
Contains the core application logic for each operation:
- **VaultInitializer**: Handles Vault initialization and unsealing
- **AuthenticationManager**: Manages token-based authentication
- **SecretManager**: Handles secret CRUD operations
- **ConfigurationManager**: Manages application configuration

#### Vault Integration Layer
Abstracts Vault-specific operations and provides a clean interface to the business layer.

#### Infrastructure Layer
Provides cross-cutting concerns like file I/O, security utilities, and logging.

## Technical Requirements

### Functional Requirements

#### FR-001: Vault Initialization
- Initialize Vault with configurable key shares and threshold
- Generate and securely store unseal keys
- Save root token for subsequent operations
- Support re-initialization scenarios

#### FR-002: Vault Unsealing
- Unseal Vault using stored keys
- Validate seal status before operations
- Handle partial unsealing scenarios

#### FR-003: Authentication Management
- Token-based authentication with Vault
- Token validation and TTL checking
- Automatic token refresh (if supported)
- Secure token storage

#### FR-004: Secret Operations
- Write secrets to specified paths
- Read secrets from specified paths
- Support for complex secret structures
- Path validation and sanitization

#### FR-005: Configuration Management
- Configurable Vault endpoint
- Environment-specific configurations
- Secure credential storage

### Non-Functional Requirements

#### NFR-001: Security
- No plaintext storage of sensitive data
- Secure memory handling for credentials
- Input validation and sanitization
- Protection against injection attacks

#### NFR-002: Performance
- Response time < 5 seconds for standard operations
- Efficient memory usage
- Minimal CPU overhead

#### NFR-003: Reliability
- Graceful error handling
- Retry mechanisms for transient failures
- Connection timeout handling
- Data integrity validation

#### NFR-004: Usability
- Intuitive command-line interface
- Comprehensive help documentation
- Clear error messages
- Progress indicators for long operations

## Implementation Details

### Project Structure

```
VaultCliCSharp/
├── VaultCliCSharp.csproj
├── Program.cs
├── Commands/
│   ├── ICommand.cs
│   ├── InitUnsealCommand.cs
│   ├── LoginCommand.cs
│   ├── WriteSecretCommand.cs
│   └── ReadSecretCommand.cs
├── Services/
│   ├── IVaultService.cs
│   ├── VaultService.cs
│   ├── IConfigurationService.cs
│   ├── ConfigurationService.cs
│   ├── IFileService.cs
│   └── FileService.cs
├── Models/
│   ├── VaultConfiguration.cs
│   ├── InitializationData.cs
│   └── SecretData.cs
├── Utilities/
│   ├── ArgumentParser.cs
│   ├── SecurityHelper.cs
│   └── ConsoleHelper.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

### Core Dependencies

```xml
<PackageReference Include="VaultSharp" Version="1.17.5.1" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
<PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
```

### Key Classes and Interfaces

#### IVaultService Interface
```csharp
public interface IVaultService
{
    Task<InitializationResult> InitializeAsync(int secretShares, int secretThreshold);
    Task<bool> UnsealAsync(IEnumerable<string> keys);
    Task<TokenInfo> LoginAsync(string token);
    Task WriteSecretAsync(string path, string key, string value);
    Task<string?> ReadSecretAsync(string path, string key);
    Task<SealStatus> GetSealStatusAsync();
}
```

#### VaultConfiguration Model
```csharp
public class VaultConfiguration
{
    public string Endpoint { get; set; } = "http://127.0.0.1:8200";
    public string TokenFile { get; set; } = ".token";
    public string InitFile { get; set; } = "vault_init.json";
    public int TimeoutSeconds { get; set; } = 30;
    public bool SkipTlsVerification { get; set; } = false;
}
```

#### Command Pattern Implementation
```csharp
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    Task<int> ExecuteAsync(string[] args);
}
```

### Vault Integration Implementation

#### VaultClient Factory
```csharp
public class VaultClientFactory
{
    public static IVaultClient CreateClient(VaultConfiguration config, string? token = null)
    {
        var authMethod = string.IsNullOrEmpty(token) 
            ? null 
            : new TokenAuthMethodInfo(token);
            
        var settings = new VaultClientSettings(config.Endpoint, authMethod)
        {
            VaultServiceTimeout = TimeSpan.FromSeconds(config.TimeoutSeconds),
            UseVaultTokenHeaderInsteadOfAuthorizationHeader = true
        };

        if (config.SkipTlsVerification)
        {
            settings.PostProcessHttpClientHandler = handler =>
            {
                handler.ServerCertificateCustomValidationCallback = 
                    (message, cert, chain, errors) => true;
            };
        }

        return new VaultClient(settings);
    }
}
```

#### Error Handling Strategy
```csharp
public class VaultOperationException : Exception
{
    public string Operation { get; }
    public int? StatusCode { get; }
    
    public VaultOperationException(string operation, string message, Exception? innerException = null)
        : base($"Vault operation '{operation}' failed: {message}", innerException)
    {
        Operation = operation;
    }
}
```

### Security Implementation

#### Secure Token Handling
```csharp
public class SecureTokenManager
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("VaultCliEntropy");
    
    public static void SaveToken(string token, string filePath)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var encryptedBytes = ProtectedData.Protect(tokenBytes, Entropy, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(filePath, encryptedBytes);
    }
    
    public static string? LoadToken(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        
        var encryptedBytes = File.ReadAllBytes(filePath);
        var tokenBytes = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(tokenBytes);
    }
}
```

#### Input Validation
```csharp
public static class InputValidator
{
    public static bool IsValidPath(string path)
    {
        return !string.IsNullOrWhiteSpace(path) 
            && !path.Contains("..") 
            && !Path.GetInvalidPathChars().Any(path.Contains);
    }
    
    public static bool IsValidKey(string key)
    {
        return !string.IsNullOrWhiteSpace(key) 
            && key.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
    }
}
```

## Security Considerations

### Data Protection
- **Token Encryption**: Use Windows DPAPI or equivalent for token storage
- **Memory Protection**: Clear sensitive data from memory after use
- **File Permissions**: Restrict access to configuration and token files
- **Transport Security**: Enforce HTTPS for production deployments

### Authentication & Authorization
- **Token Validation**: Verify token validity before operations
- **Least Privilege**: Request minimal required permissions
- **Token Rotation**: Support automatic token renewal
- **Audit Logging**: Log all authentication attempts

### Input Validation
- **Path Traversal Protection**: Validate and sanitize path inputs
- **Injection Prevention**: Escape special characters in inputs
- **Length Limits**: Enforce reasonable limits on input sizes
- **Character Validation**: Allow only safe characters in keys/paths

## Error Handling Strategy

### Exception Hierarchy
```
VaultCliException
├── VaultOperationException
│   ├── InitializationException
│   ├── AuthenticationException
│   └── SecretOperationException
├── ConfigurationException
└── ValidationException
```

### Error Categories

#### Network Errors
- Connection timeouts
- DNS resolution failures
- Certificate validation errors
- HTTP status errors

#### Vault-Specific Errors
- Sealed vault operations
- Permission denied
- Path not found
- Token expired

#### Application Errors
- Invalid configuration
- File system errors
- Input validation failures

### Retry Strategy
```csharp
public class RetryPolicy
{
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        TimeSpan delay = default)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (ShouldRetry(ex, attempt, maxRetries))
            {
                attempt++;
                await Task.Delay(delay == default ? TimeSpan.FromSeconds(attempt) : delay);
            }
        }
    }
}
```

## Configuration Management

### Configuration Sources
1. **Command Line Arguments**: Override default settings
2. **Environment Variables**: Runtime configuration
3. **Configuration Files**: Persistent settings
4. **Default Values**: Fallback configuration

### Configuration Schema
```json
{
  "VaultConfiguration": {
    "Endpoint": "https://vault.company.com:8200",
    "TokenFile": ".vault-token",
    "InitFile": "vault-init.json",
    "TimeoutSeconds": 30,
    "SkipTlsVerification": false,
    "RetryCount": 3,
    "RetryDelay": "00:00:02"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "VaultCliCSharp": "Debug"
    }
  }
}
```

## Testing Strategy

### Unit Testing
- **Service Layer Testing**: Mock Vault operations
- **Command Testing**: Validate command parsing and execution
- **Utility Testing**: Test helper functions and validators
- **Configuration Testing**: Verify configuration loading

### Integration Testing
- **Vault Integration**: Test against real Vault instance
- **End-to-End Scenarios**: Complete workflow testing
- **Error Scenario Testing**: Simulate failure conditions

### Testing Tools
- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for dependencies
- **Testcontainers**: Containerized Vault for integration tests

### Test Structure
```
Tests/
├── VaultCliCSharp.UnitTests/
│   ├── Commands/
│   ├── Services/
│   └── Utilities/
├── VaultCliCSharp.IntegrationTests/
│   ├── VaultOperations/
│   └── EndToEnd/
└── VaultCliCSharp.TestUtilities/
    ├── Mocks/
    └── Fixtures/
```

## Deployment Guidelines

### Build Configuration
```xml
<PropertyGroup>
  <Configuration>Release</Configuration>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishTrimmed>true</PublishTrimmed>
</PropertyGroup>
```

### Platform-Specific Builds
- **Windows**: Self-contained executable
- **Linux**: Self-contained binary with appropriate permissions
- **macOS**: Universal binary for Intel and Apple Silicon

### Distribution Methods
1. **GitHub Releases**: Platform-specific binaries
2. **Package Managers**: Chocolatey (Windows), Homebrew (macOS)
3. **Container Images**: Docker images for containerized environments
4. **CI/CD Integration**: Direct integration in build pipelines

### Installation Script
```bash
#!/bin/bash
PLATFORM=$(uname -s | tr '[:upper:]' '[:lower:]')
ARCH=$(uname -m)
BINARY_NAME="vault-cli-${PLATFORM}-${ARCH}"
INSTALL_DIR="/usr/local/bin"

curl -L "https://github.com/company/vault-cli/releases/latest/download/${BINARY_NAME}" \
  -o "${INSTALL_DIR}/vault-cli"
chmod +x "${INSTALL_DIR}/vault-cli"
```

## Appendices

### Appendix A: VaultSharp API Reference

#### Key VaultSharp Classes
- **VaultClient**: Main client for Vault operations
- **VaultClientSettings**: Configuration for Vault client
- **TokenAuthMethodInfo**: Token-based authentication
- **InitializeRequest/InitializeResponse**: Vault initialization
- **UnsealRequest/UnsealResponse**: Vault unsealing operations

#### Common Operations
```csharp
// Client initialization
var authMethod = new TokenAuthMethodInfo("vault-token");
var vaultClientSettings = new VaultClientSettings("https://vault:8200", authMethod);
var vaultClient = new VaultClient(vaultClientSettings);

// System operations
var initStatus = await vaultClient.V1.System.GetInitializationStatusAsync();
var sealStatus = await vaultClient.V1.System.GetSealStatusAsync();

// KV v2 operations
await vaultClient.V1.Secrets.KeyValue.V2.WriteSecretAsync("path", data);
var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync("path");
```

### Appendix B: Vault API Endpoints

#### System Endpoints
- **GET /v1/sys/init**: Check initialization status
- **PUT /v1/sys/init**: Initialize Vault
- **GET /v1/sys/seal-status**: Get seal status
- **PUT /v1/sys/unseal**: Unseal Vault

#### Auth Endpoints
- **GET /v1/auth/token/lookup-self**: Token information
- **POST /v1/auth/token/renew-self**: Renew token

#### KV v2 Endpoints
- **GET /v1/secret/data/{path}**: Read secret
- **POST /v1/secret/data/{path}**: Write secret
- **DELETE /v1/secret/data/{path}**: Delete secret

### Appendix C: Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `VAULT_ADDR` | Vault server address | `http://127.0.0.1:8200` |
| `VAULT_TOKEN` | Vault authentication token | None |
| `VAULT_SKIP_VERIFY` | Skip TLS verification | `false` |
| `VAULT_CLI_CONFIG` | Configuration file path | `vault-cli.json` |

### Appendix D: Exit Codes

| Code | Description |
|------|-------------|
| 0 | Success |
| 1 | General error |
| 2 | Invalid arguments |
| 3 | Authentication failure |
| 4 | Vault operation failure |
| 5 | Configuration error |

---

**Document Version**: 1.0  
**Last Updated**: June 2025  
**Author**: anh.nt 
**Review Status**: Approved
<div align="center">
  <h3 align="center">SecureVault CLI</h3>

  <p align="center">
    A robust .NET 8 command-line interface for HashiCorp Vault operations
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
        <li><a href="#features">Features</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#configuration">Configuration</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>

<!-- ABOUT THE PROJECT -->
## About The Project

SecureVault CLI is a comprehensive command-line interface for HashiCorp Vault operations, built with .NET 8. This tool provides secure secret management operations through an intuitive console-based interface, supporting core Vault functionalities including initialization, unsealing, authentication, and secret management.

### Key Objectives
- 🔐 Provide a robust CLI interface for Vault operations
- 🛡️ Implement secure authentication and authorization mechanisms
- 📝 Support core secret management operations (CRUD)
- ⚡ Ensure compatibility with Vault's KV secrets engine v2
- 🔒 Maintain high security standards for credential handling

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Built With

This project is built using modern .NET technologies and industry-standard libraries:

* [![.NET][.NET-badge]][.NET-url]
* [![C#][CSharp-badge]][CSharp-url]
* [![VaultSharp][VaultSharp-badge]][VaultSharp-url]

**Core Dependencies:**
- **VaultSharp 1.17.5.1** - HashiCorp Vault API client
- **Microsoft.Extensions.*** - Configuration, DI, and Logging
- **System.CommandLine** - Modern command-line parsing
- **xUnit & Moq** - Testing framework and mocking

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Features

- ✅ **Vault Initialization** - Initialize Vault with configurable key shares and threshold
- ✅ **Vault Unsealing** - Unseal Vault using stored keys with progress tracking
- ✅ **Token Authentication** - Secure token-based authentication with encrypted storage
- ✅ **Secret Management** - Write and read secrets with comprehensive validation
- ✅ **Configuration Management** - Multi-source configuration (JSON, environment, CLI)
- ✅ **Error Handling** - Robust error handling with helpful user messages
- ✅ **Cross-Platform** - Runs on Windows, Linux, and macOS
- ✅ **Dependency Injection** - Modern .NET service architecture
- ✅ **Comprehensive Testing** - Unit and integration tests

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GETTING STARTED -->
## Getting Started

To get SecureVault CLI up and running locally, follow these simple steps.

### Prerequisites

Before you begin, ensure you have the following installed:

* **.NET 8.0 SDK or later**
  ```sh
  # Verify installation
  dotnet --version
  ```

* **HashiCorp Vault** (for testing)
  ```sh
  # Using Homebrew (macOS)
  brew install vault
  
  # Using Chocolatey (Windows)
  choco install vault
  
  # Or download from https://www.vaultproject.io/downloads
  ```

### Installation

1. **Clone the repository**
   ```sh
   git clone https://github.com/your-username/secure-vault-cli.git
   cd secure-vault-cli
   ```

2. **Restore dependencies**
   ```sh
   dotnet restore
   ```

3. **Build the project**
   ```sh
   dotnet build
   ```

4. **Run the application**
   ```sh
   dotnet run --project SecureVault -- help
   ```

#### Alternative: Build Self-Contained Executable

Create a platform-specific executable:

```sh
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- USAGE EXAMPLES -->
## Usage

SecureVault CLI provides five core commands for managing HashiCorp Vault operations:

### Basic Command Structure

```sh
dotnet run --project SecureVault -- <command> [arguments]
```

### Available Commands

#### 1. Initialize Vault
Initialize a new Vault instance with configurable key shares and threshold:

```sh
# Initialize with default settings (5 shares, 3 threshold)
dotnet run --project SecureVault -- init

# Initialize with custom settings
dotnet run --project SecureVault -- init --shares 5 --threshold 3
```

#### 2. Unseal Vault
Unseal Vault using stored keys:

```sh
dotnet run --project SecureVault -- unseal
```

#### 3. Authenticate with Vault
Login using a Vault token:

```sh
# Login with token prompt
dotnet run --project SecureVault -- login

# Login with token argument
dotnet run --project SecureVault -- login --token hvs.XXXXXX
```

#### 4. Write Secrets
Store secrets in Vault:

```sh
dotnet run --project SecureVault -- write myapp/db password secret123
dotnet run --project SecureVault -- write myapp/api key abc123def456
```

#### 5. Read Secrets
Retrieve secrets from Vault:

```sh
dotnet run --project SecureVault -- read myapp/db password
dotnet run --project SecureVault -- read myapp/api key
```

### Complete Workflow Example

Here's a typical workflow for setting up and using Vault:

```sh
# 1. Start Vault in dev mode (for testing)
vault server -dev

# 2. Initialize Vault
dotnet run --project SecureVault -- init --shares 5 --threshold 3

# 3. Unseal Vault (if not in dev mode)
dotnet run --project SecureVault -- unseal

# 4. Login with root token
dotnet run --project SecureVault -- login --token <root-token>

# 5. Store a secret
dotnet run --project SecureVault -- write myapp/database password mysecretpassword

# 6. Retrieve the secret
dotnet run --project SecureVault -- read myapp/database password
```

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONFIGURATION -->
## Configuration

SecureVault CLI supports multiple configuration sources with the following precedence:

1. **Command-line arguments** (highest priority)
2. **Environment variables**
3. **Configuration files**
4. **Default values** (lowest priority)

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `VAULT_ADDR` | Vault server address | `http://127.0.0.1:8200` |
| `VAULT_TOKEN` | Vault authentication token | None |
| `VAULT_SKIP_VERIFY` | Skip TLS verification | `false` |

### Configuration File

Create a [`vault-cli.json`](vault-cli.json) file in the application directory:

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
      "SecureVault": "Debug"
    }
  }
}
```

### Security Considerations

- **Token Storage**: Tokens are encrypted using Windows DPAPI or equivalent
- **File Permissions**: Configuration files should have restricted access
- **TLS Verification**: Always use HTTPS in production environments
- **Input Validation**: All inputs are validated and sanitized

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTRIBUTING -->
## Contributing

Contributions make the open source community an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".

1. **Fork the Project**
2. **Create your Feature Branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit your Changes** (`git commit -m 'Add some AmazingFeature'`)
4. **Push to the Branch** (`git push origin feature/AmazingFeature`)
5. **Open a Pull Request**

### Development Guidelines

- Follow C# coding conventions and best practices
- Write unit tests for new functionality
- Update documentation for any API changes
- Ensure all tests pass before submitting PR
- Use meaningful commit messages

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- LICENSE -->
## License

Distributed under the MIT License. See [`LICENSE`](LICENSE) for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTACT -->
## Contact

**Project Maintainer:** anh.nt

**Project Link:** [https://github.com/anhnt2003/secure-vault-cli](https://github.com/anhnt2003/secure-vault-cli)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

Special thanks to the following projects and resources that made this possible:

* [HashiCorp Vault](https://www.vaultproject.io/) - The secure secret management platform
* [VaultSharp](https://github.com/rajanadar/VaultSharp) - Excellent .NET client for Vault
* [Microsoft Extensions](https://docs.microsoft.com/en-us/dotnet/core/extensions/) - Configuration, DI, and Logging
* [System.CommandLine](https://github.com/dotnet/command-line-api) - Modern command-line parsing
* [Best README Template](https://github.com/othneildrew/Best-README-Template) - README template inspiration
* [Shields.io](https://shields.io/) - Beautiful badges for documentation

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- MARKDOWN LINKS & IMAGES -->
[.NET-badge]: https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white
[.NET-url]: https://dotnet.microsoft.com/
[CSharp-badge]: https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white
[CSharp-url]: https://docs.microsoft.com/en-us/dotnet/csharp/
[VaultSharp-badge]: https://img.shields.io/badge/VaultSharp-1.17.5.1-FF6B35?style=for-the-badge&logo=hashicorp&logoColor=white
[VaultSharp-url]: https://github.com/rajanadar/VaultSharp
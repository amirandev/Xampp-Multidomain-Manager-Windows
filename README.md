# Xampp Multidomain Manager

A Windows desktop application that provides a graphical interface for managing multiple local domains (virtual hosts) on a XAMPP installation. No more manually editing Apache config files, hosts file, or MySQL databases — everything is handled through a clean WinUI 3 dashboard.

<img width="1450" height="751" alt="image" src="https://github.com/user-attachments/assets/f5db6b29-ae34-4c76-a8c8-1d0615ff6c8a" />

## Requirements

- **Windows 10** (version 1809 or later) or **Windows 11**
- **XAMPP** latest version installed at `C:\xampp` (Apache + MySQL)
- .NET SDK 10.0 (for development/debugging)

## Features

### Dashboard
- Start/Stop Apache and MySQL services
- Real-time service status indicators
- Apply configuration with a single click
- Overview of total domains and databases

### Domains (Virtual Hosts)
- Add, edit, rename, and delete virtual hosts
- Enable/disable domains individually
- Auto-generates document root with boilerplate `index.php`
- Server alias support
- Automatically manages Apache `httpd-vhosts.conf` and SSL config
- Creates self-signed SSL certificates for HTTPS support
- Manages Windows `hosts` file entries
- Open domain in browser or document root in Explorer
<img width="1432" height="741" alt="image" src="https://github.com/user-attachments/assets/f56b3d78-ccf4-4c16-9118-7509651cbe60" />

### Databases
- List all MySQL databases
- Create databases with a dedicated user and password
- Delete databases with confirmation
- Open phpMyAdmin globally or deep-linked to a specific database
- MySQL user management
<img width="1432" height="742" alt="image" src="https://github.com/user-attachments/assets/f2cd131f-93ae-42a3-a353-91c0eea5a2c1" />


### PHP Ini Editor
- Load, edit, and save `php.ini` directly from the app
- Reload from disk and save with admin privileges
<img width="1407" height="715" alt="image" src="https://github.com/user-attachments/assets/2e04923d-04bf-404a-b5a0-d4edd30b2730" />


### Settings
- Verify XAMPP installation path

## How It Works

The app stores virtual host records in a local SQLite database (`%LOCALAPPDATA%\XamppMultidomainManager`). When you add a domain, it:

1. Creates the document root folder with a starter `index.php`
2. Generates a self-signed SSL certificate via OpenSSL
3. Adds the domain to Apache's virtual host configuration
4. Adds the domain to the Windows `hosts` file (`127.0.0.1`)
5. Restarts Apache to apply changes

All operations run with full administrator privileges to allow writing to system-protected files.

## Tech Stack

| Component | Technology |
|---|---|
| UI Framework | **WinUI 3** (Windows App SDK 2.2) |
| Language | **C#** with .NET 10 |
| Architecture | MVVM pattern |
| Database (local) | **SQLite** via `Microsoft.Data.Sqlite` |
| Database (remote) | **MySQL** via XAMPP's bundled CLI tools |
| SSL | **OpenSSL** (bundled with XAMPP) |
| Packaging | MSIX (Windows App SDK) |

## Development

### Prerequisites
- Visual Studio 2022+ with **Windows App SDK** workload
- .NET 10.0 SDK
- XAMPP installed at `C:\xampp`

### Debugging (run.bat)
The included `run.bat` script handles running the app with elevated privileges:

```bat
.\run.bat
```

It auto-elevates to Administrator (required for modifying `hosts` file and Apache config).

### Build & Run
```bash
dotnet run
```

Or open `XamppMultidomainManager.csproj` in Visual Studio.

## License

All rights reserved. This software is provided for use by the XAMPP / Apache Friends project. No permission is granted for modification, distribution, or commercial use without explicit written consent from XROW.ASIA.

## Contact

XROW.ASIA — amoswaper@gmail.com

# Xampp Multidomain Manager

A Windows desktop application that provides a graphical interface for managing multiple local domains (virtual hosts) on a XAMPP installation. No more manually editing Apache config files, hosts file, or MySQL databases — everything is handled through a clean WinUI 3 dashboard.

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

### Databases
- List all MySQL databases
- Create databases with a dedicated user and password
- Delete databases with confirmation
- Open phpMyAdmin globally or deep-linked to a specific database
- MySQL user management

### PHP Ini Editor
- Load, edit, and save `php.ini` directly from the app
- Reload from disk and save with admin privileges

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
| Packaging | Single-file self-extracting (WinUI unpackaged) |

## Scripts

### `build-installable.bat`
Builds the project as a single-file distributable `.exe` and copies it to `build-installable\`. If the Windows SDK is installed, it also signs the executable with a self-signed certificate.

```bat
.\build-installable.bat
```

Output: `build-installable\XamppMultidomainManager.exe`

### `install.bat`
Builds and installs the app to `C:\Program Files\XamppMultidomainManager\`, creates Start Menu and Desktop shortcuts.

```bat
.\install.bat
```

### `run-build.bat`
Builds the Release version, copies the output to `build\`, and launches the app.

```bat
.\run-build.bat
```

### `rundebug.bat`
Builds and runs the Debug version (same as `dotnet run`) with auto-elevation.

```bat
.\rundebug.bat
```

### `run.bat`
Legacy script — same as `rundebug.bat`.

### `generate_icons.py`
Python script that generates all icon sizes (`16px.ico` through `96px.ico`) and app assets from `256px.ico` on a white background.

```bash
pip install Pillow
python generate_icons.py
```

## First-Run Setup

When you launch the app for the first time (from the distributable `.exe`), it detects it's not installed and shows a setup wizard that:

1. Copies the executable to `C:\Program Files\XamppMultidomainManager\`
2. Creates a Start Menu shortcut
3. Launches the installed version

On subsequent runs, the app starts normally.

## Development

### Prerequisites
- Visual Studio 2022+ with **Windows App SDK** workload
- .NET 10.0 SDK
- XAMPP installed at `C:\xampp`

### Debug
```bash
dotnet run
```

Or open `XamppMultidomainManager.csproj` in Visual Studio and press F5.

## License

All rights reserved. This software is provided for use by the XAMPP / Apache Friends project. No permission is granted for modification, distribution, or commercial use without explicit written consent from XROW.ASIA.

## Contact

XROW.ASIA — amoswaper@gmail.com

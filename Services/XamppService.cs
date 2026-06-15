/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using System.Diagnostics;

namespace XamppMultidomainManager.Services;

public class XamppService
{
    public string XamppPath => @"C:\xampp";
    private string MysqlExe => Path.Combine(XamppPath, "mysql", "bin", "mysql.exe");
    private string MysqlDumpExe => Path.Combine(XamppPath, "mysql", "bin", "mysqldump.exe");

    public bool GetApacheStatus()
    {
        return GetServiceStatus("Apache2.4") || GetProcessStatus("httpd");
    }

    public bool GetMySqlStatus()
    {
        return GetServiceStatus("mysql") || GetProcessStatus("mysqld");
    }

    private static bool GetServiceStatus(string serviceName)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"query \"{serviceName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(3000);
            return output.Contains("RUNNING");
        }
        catch { return false; }
    }

    private static bool GetProcessStatus(string processName)
    {
        try { return Process.GetProcessesByName(processName).Length > 0; }
        catch { return false; }
    }

    public async Task<(bool Success, string Message)> StartApache() =>
        await StartBat(Path.Combine(XamppPath, "apache_start.bat"), "Apache");
    public async Task<(bool Success, string Message)> StopApache()
    {
        if (GetServiceStatus("Apache2.4"))
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "net",
                    Arguments = "stop Apache2.4",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var process = new Process { StartInfo = psi };
                process.Start();
                var error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit(10000);
                if (process.ExitCode != 0)
                    return (false, $"Apache service stop failed: {error.Trim()}");
                await Task.Delay(2000);
                return (true, "Apache service stopped");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to stop Apache service: {ex.Message}");
            }
        }
        return await RunExe(Path.Combine(XamppPath, "apache", "bin", "httpd.exe"), "-k shutdown", "Apache", 10000);
    }
    public async Task<(bool Success, string Message)> StartMySql() =>
        await StartBat(Path.Combine(XamppPath, "mysql_start.bat"), "MySQL");
    public async Task<(bool Success, string Message)> StopMySql() =>
        await RunExe(Path.Combine(XamppPath, "mysql", "bin", "mysqladmin.exe"), "-u root shutdown", "MySQL", 10000);

    public (bool Valid, string Message) TestConfig()
    {
        try
        {
            var httpd = Path.Combine(XamppPath, "apache", "bin", "httpd.exe");
            if (!File.Exists(httpd))
                return (false, "httpd.exe not found");

            var psi = new ProcessStartInfo
            {
                FileName = httpd,
                Arguments = "-t",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using var process = new Process { StartInfo = psi };
            process.Start();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(10000);

            if (process.ExitCode != 0)
                return (false, error.Trim());

            return (true, "Syntax OK");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> GracefulReloadApache()
    {
        try
        {
            var httpd = Path.Combine(XamppPath, "apache", "bin", "httpd.exe");
            if (!File.Exists(httpd))
                return (false, "httpd.exe not found");

            // First validate config
            var (valid, msg) = TestConfig();
            if (!valid)
                return (false, $"Config test failed: {msg}");

            var psi = new ProcessStartInfo
            {
                FileName = httpd,
                Arguments = "-k graceful",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using var process = new Process { StartInfo = psi };
            process.Start();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(10000);

            if (process.ExitCode != 0)
                return (false, $"Graceful reload failed: {error.Trim()}");

            return (true, "Apache reloaded gracefully");
        }
        catch (Exception ex)
        {
            Logger.Log("GracefulReloadApache", ex);
            return (false, ex.Message);
        }
    }

    private static async Task<(bool, string)> StartBat(string filePath, string name)
    {
        try
        {
            if (!File.Exists(filePath))
                return (false, $"{name} script not found at {filePath}");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filePath, UseShellExecute = true,
                    CreateNoWindow = false, WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            process.Start();
            await Task.Delay(2000);
            if (process.HasExited && process.ExitCode != 0)
                return (false, $"{name} failed with exit code {process.ExitCode}");
            return (true, $"{name} command executed successfully");
        }
        catch (Exception ex) { Logger.Log($"StartBat({name})", ex); return (false, $"Failed to {name}: {ex.Message}"); }
    }

    private static async Task<(bool, string)> RunExe(string exePath, string args, string name, int timeoutMs = 10000)
    {
        try
        {
            if (!File.Exists(exePath))
                return (false, $"{exePath} not found");
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using var process = new Process { StartInfo = psi };
            process.Start();
            var error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit(timeoutMs);
            if (process.ExitCode != 0)
                return (false, $"{name} failed: {error.Trim()}");
            return (true, $"{name} stopped successfully");
        }
        catch (Exception ex) { Logger.Log($"RunExe({name})", ex); return (false, $"Failed to stop {name}: {ex.Message}"); }
    }

    // ── Database CRUD ──────────────────────────────────────────────

    public List<string> GetDatabases()
    {
        var list = new List<string>();
        if (!File.Exists(MysqlExe)) return list;
        try
        {
            var output = RunMysql("SHOW DATABASES;");
            foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                var db = line.Trim();
                if (db != "" && db != "information_schema" && db != "performance_schema" && db != "mysql" && db != "sys")
                    list.Add(db);
            }
        }
        catch { }
        return list;
    }

    public async Task<(bool, string)> CreateDatabase(string dbName) =>
        await RunMysqlAsync($"CREATE DATABASE IF NOT EXISTS `{dbName}` CHARACTER SET utf8 COLLATE utf8_general_ci;",
            $"Database '{dbName}' created");

    public async Task<(bool, string)> DeleteDatabase(string dbName) =>
        await RunMysqlAsync($"DROP DATABASE IF EXISTS `{dbName}`;",
            $"Database '{dbName}' deleted");

    // ── Backup / Import ────────────────────────────────────────────

    public async Task<(bool, string)> BackupDatabase(string dbName, string outputPath)
    {
        if (!File.Exists(MysqlDumpExe)) return (false, "mysqldump not found");
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = MysqlDumpExe,
                Arguments = $"-u root --routines --triggers \"{dbName}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var process = new Process { StartInfo = psi };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit(30000);
            if (process.ExitCode != 0) return (false, error.Trim());
            await File.WriteAllTextAsync(outputPath, output);
            return (true, $"Backup saved to {outputPath}");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool, string)> ImportDatabase(string dbName, string inputPath)
    {
        if (!File.Exists(inputPath)) return (false, "File not found");
        if (!File.Exists(MysqlExe)) return (false, "mysql not found");
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = MysqlExe,
                Arguments = $"-u root \"{dbName}\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var process = new Process { StartInfo = psi };
            process.Start();
            var sql = await File.ReadAllTextAsync(inputPath);
            await process.StandardInput.WriteAsync(sql);
            process.StandardInput.Close();
            var error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit(60000);
            if (process.ExitCode != 0) return (false, error.Trim());
            return (true, $"Import completed for {dbName}");
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    // ── User Management ────────────────────────────────────────────

    public List<string> GetUsers()
    {
        var users = new List<string>();
        if (!File.Exists(MysqlExe)) return users;
        try
        {
            var output = RunMysql("SELECT CONCAT(User, '@', Host) FROM mysql.user ORDER BY User;");
            foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                var u = line.Trim();
                if (u != "") users.Add(u);
            }
        }
        catch { }
        return users;
    }

    public async Task<(bool, string)> CreateUser(string username, string password) =>
        await RunMysqlAsync($"CREATE USER IF NOT EXISTS '{username}'@'localhost' IDENTIFIED BY '{password}';" +
                            $" GRANT ALL PRIVILEGES ON *.* TO '{username}'@'localhost'; FLUSH PRIVILEGES;",
            $"User '{username}' created");

    public async Task<(bool, string)> SetPassword(string username, string password) =>
        await RunMysqlAsync($"SET PASSWORD FOR '{username}'@'localhost' = PASSWORD('{password}'); FLUSH PRIVILEGES;",
            $"Password updated for '{username}'");

    public async Task<(bool, string)> DeleteUser(string username)
    {
        var host = username.Contains('@') ? null : "localhost";
        var user = username.Contains('@') ? username.Split('@')[0] : username;
        var h = host ?? username.Split('@')[1];
        return await RunMysqlAsync($"DROP USER IF EXISTS '{user}'@'{h}'; FLUSH PRIVILEGES;",
            $"User '{username}' deleted");
    }

    public async Task<(bool, string)> GrantDbAccess(string username, string database) =>
        await RunMysqlAsync($"GRANT ALL PRIVILEGES ON `{database}`.* TO '{username}'@'localhost'; FLUSH PRIVILEGES;",
            $"Granted access to '{database}' for '{username}'");

    public async Task<(bool, string)> GrantPrivileges(string username, string database) =>
        await GrantDbAccess(username, database);

    public async Task<(bool, string)> CreateDatabaseUser(string username, string password, string database) =>
        await RunMysqlAsync(
            $"CREATE USER IF NOT EXISTS '{username}'@'localhost' IDENTIFIED BY '{password}';" +
            $"GRANT ALL PRIVILEGES ON `{database}`.* TO '{username}'@'localhost';" +
            $"FLUSH PRIVILEGES;",
            $"Created user '{username}' for database '{database}'");

    // ── Helpers ────────────────────────────────────────────────────

    private string RunMysql(string args)
    {
        using var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = MysqlExe, Arguments = $"-u root -e \"{args}\"",
                UseShellExecute = false, RedirectStandardOutput = true,
                RedirectStandardError = true, CreateNoWindow = true
            }
        };
        p.Start();
        var o = p.StandardOutput.ReadToEnd();
        p.WaitForExit(10000);
        return o;
    }

    private async Task<(bool, string)> RunMysqlAsync(string sql, string okMsg)
    {
        if (!File.Exists(MysqlExe)) return (false, "MySQL binary not found");
        try
        {
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = MysqlExe, Arguments = $"-u root -e \"{sql}\"",
                    UseShellExecute = false, RedirectStandardOutput = true,
                    RedirectStandardError = true, CreateNoWindow = true
                }
            };
            p.Start();
            var err = await p.StandardError.ReadToEndAsync();
            p.WaitForExit(10000);
            if (p.ExitCode != 0) return (false, err.Trim());
            return (true, okMsg);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}

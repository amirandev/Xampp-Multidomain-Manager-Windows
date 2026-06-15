/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using Microsoft.Data.Sqlite;
using XamppMultidomainManager.Models;

namespace XamppMultidomainManager.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XamppMultidomainManager", "XamppMultidomainManager.db");
        var dbDir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
            Directory.CreateDirectory(dbDir);

        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS VirtualHosts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DomainName TEXT NOT NULL UNIQUE,
                DocumentRoot TEXT NOT NULL,
                ServerAlias TEXT,
                Enabled INTEGER DEFAULT 1,
                CreatedAt TEXT,
                UpdatedAt TEXT
            )
            """;
        cmd.ExecuteNonQuery();
    }

    public List<VirtualHost> GetAllHosts()
    {
        var hosts = new List<VirtualHost>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM VirtualHosts ORDER BY DomainName";
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            hosts.Add(MapReaderToHost(reader));
        }

        return hosts;
    }

    public VirtualHost? GetHostById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM VirtualHosts WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
            return MapReaderToHost(reader);

        return null;
    }

    public VirtualHost? GetHostByDomain(string domain)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM VirtualHosts WHERE DomainName = @domain";
        cmd.Parameters.AddWithValue("@domain", domain);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
            return MapReaderToHost(reader);

        return null;
    }

    public void ImportHosts(List<VirtualHost> hosts)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        foreach (var host in hosts)
        {
            var existing = GetHostByDomain(host.DomainName);
            if (existing != null)
                continue;

            var cmd = connection.CreateCommand();
            cmd.CommandText = """
                INSERT OR IGNORE INTO VirtualHosts (DomainName, DocumentRoot, ServerAlias, Enabled, CreatedAt, UpdatedAt)
                VALUES (@domain, @root, @alias, @enabled, @created, @updated)
                """;
            cmd.Parameters.AddWithValue("@domain", host.DomainName);
            cmd.Parameters.AddWithValue("@root", host.DocumentRoot);
            cmd.Parameters.AddWithValue("@alias", (object?)host.ServerAlias ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@enabled", host.Enabled ? 1 : 0);
            cmd.Parameters.AddWithValue("@created", host.CreatedAt);
            cmd.Parameters.AddWithValue("@updated", host.UpdatedAt);
            cmd.ExecuteNonQuery();
        }
    }

    public void AddHost(VirtualHost host)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO VirtualHosts (DomainName, DocumentRoot, ServerAlias, Enabled, CreatedAt, UpdatedAt)
            VALUES (@domain, @root, @alias, @enabled, @created, @updated)
            """;
        cmd.Parameters.AddWithValue("@domain", host.DomainName);
        cmd.Parameters.AddWithValue("@root", host.DocumentRoot);
        cmd.Parameters.AddWithValue("@alias", (object?)host.ServerAlias ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@enabled", host.Enabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@created", host.CreatedAt);
        cmd.Parameters.AddWithValue("@updated", host.UpdatedAt);
        cmd.ExecuteNonQuery();
    }

    public void UpdateHost(VirtualHost host)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE VirtualHosts SET DomainName = @domain, DocumentRoot = @root,
                ServerAlias = @alias, Enabled = @enabled, UpdatedAt = @updated
            WHERE Id = @id
            """;
        cmd.Parameters.AddWithValue("@id", host.Id);
        cmd.Parameters.AddWithValue("@domain", host.DomainName);
        cmd.Parameters.AddWithValue("@root", host.DocumentRoot);
        cmd.Parameters.AddWithValue("@alias", (object?)host.ServerAlias ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@enabled", host.Enabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@updated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.ExecuteNonQuery();
    }

    public void DeleteHost(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM VirtualHosts WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static VirtualHost MapReaderToHost(SqliteDataReader reader)
    {
        return new VirtualHost
        {
            Id = reader.GetInt32(0),
            DomainName = reader.GetString(1),
            DocumentRoot = reader.GetString(2),
            ServerAlias = reader.IsDBNull(3) ? null : reader.GetString(3),
            Enabled = reader.GetInt32(4) == 1,
            CreatedAt = reader.GetString(5),
            UpdatedAt = reader.GetString(6)
        };
    }
}

/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using System.Diagnostics;

namespace XamppMultidomainManager.Services;

public class SslService
{
    private readonly XamppService _xamppService;
    public string XamppPath => _xamppService.XamppPath;

    public SslService(XamppService xamppService)
    {
        _xamppService = xamppService;
    }

    public string CertDir => Path.Combine(XamppPath, "apache", "conf", "ssl.crt");
    public string KeyDir => Path.Combine(XamppPath, "apache", "conf", "ssl.key");

    public bool CertificateExists(string domain)
    {
        return File.Exists(Path.Combine(CertDir, $"{domain}.crt")) &&
               File.Exists(Path.Combine(KeyDir, $"{domain}.key"));
    }

    public async Task<(bool Success, string Message)> GenerateCertificate(string domain)
    {
        var opensslPath = Path.Combine(XamppPath, "apache", "bin", "openssl.exe");
        if (!File.Exists(opensslPath))
            return (false, "OpenSSL not found");

        try
        {
            if (!Directory.Exists(CertDir)) Directory.CreateDirectory(CertDir);
            if (!Directory.Exists(KeyDir)) Directory.CreateDirectory(KeyDir);

            var keyFile = Path.Combine(KeyDir, $"{domain}.key");
            var certFile = Path.Combine(CertDir, $"{domain}.crt");

            var configPath = ResolveOpensslCnf();
            var configArg = configPath != null ? $" -config \"{configPath}\"" : "";

            var (success, message) = await RunOpenssl(opensslPath, domain, keyFile, certFile, configArg, true);

            if (!success && configPath == null && message.Contains("Can't open"))
            {
                var tmpConfig = await CreateTempConfig();
                if (tmpConfig != null)
                    (success, message) = await RunOpenssl(opensslPath, domain, keyFile, certFile, tmpConfig, true);
            }

            if (!success && message.Contains("Unknown option") && message.Contains("addext"))
                (success, message) = await RunOpenssl(opensslPath, domain, keyFile, certFile, configArg, false);

            if (!success)
                return (false, message);

            return (true, $"SSL certificate generated for {domain}");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to generate certificate: {ex.Message}");
        }
    }

    private static async Task<(bool, string)> RunOpenssl(string opensslPath, string domain,
        string keyFile, string certFile, string configArg, bool useAddext)
    {
        var addext = useAddext ? $" -addext \"subjectAltName=DNS:{domain}\"" : "";
        var psi = new ProcessStartInfo
        {
            FileName = opensslPath,
            Arguments = $"req -x509 -nodes -days 3650 -newkey rsa:2048{configArg}" +
                        $" -keyout \"{keyFile}\" -out \"{certFile}\"" +
                        $" -subj \"/C=US/ST=Local/L=Local/O=XROW.ASIA/CN={domain}\"" +
                        addext,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();
        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit(15000);

        if (process.ExitCode != 0)
            return (false, error.Trim());

        return (true, "");
    }

    private string? ResolveOpensslCnf()
    {
        var candidates = new[]
        {
            Path.Combine(XamppPath, "apache", "conf", "openssl.cnf"),
            Path.Combine(XamppPath, "apache", "bin", "openssl.cnf"),
            Path.Combine(XamppPath, "openssl.cnf"),
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    private async Task<string?> CreateTempConfig()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "XamppMultidomainManager");
        if (!Directory.Exists(tmpDir)) Directory.CreateDirectory(tmpDir);

        var tmpCnf = Path.Combine(tmpDir, "openssl.cnf");
        await File.WriteAllTextAsync(tmpCnf, """
            [req]
            distinguished_name = req_distinguished_name
            x509_extensions = v3_req
            prompt = no
            [req_distinguished_name]
            CN = placeholder
            [v3_req]
            keyUsage = keyEncipherment, dataEncipherment
            extendedKeyUsage = serverAuth
            subjectAltName = @alt_names
            [alt_names]
            DNS.1 = placeholder
            """);

        return $" -config \"{tmpCnf}\"";
    }

    public void DeleteCertificate(string domain)
    {
        var certFile = Path.Combine(CertDir, $"{domain}.crt");
        var keyFile = Path.Combine(KeyDir, $"{domain}.key");
        if (File.Exists(certFile)) File.Delete(certFile);
        if (File.Exists(keyFile)) File.Delete(keyFile);
    }
}

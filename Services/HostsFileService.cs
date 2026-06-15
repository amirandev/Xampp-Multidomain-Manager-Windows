/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
namespace XamppMultidomainManager.Services;

public class HostsFileService
{
    private static readonly string HostsFilePath = @"C:\Windows\System32\drivers\etc\hosts";
    private const string SectionStart = "# XROW.ASIA :: XamppMultidomainManager hosts section START----------";
    private const string SectionEnd = "# XROW.ASIA :: XamppMultidomainManager hosts section END  ----------";

    public bool DomainExists(string domain)
    {
        try
        {
            var lines = File.ReadAllLines(HostsFilePath).ToList();
            return GetManagedLines(lines).Any(l =>
                l.Contains(domain, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    public void AddDomain(string domain, string ip = "127.0.0.1")
    {
        try
        {
            var lines = File.ReadAllLines(HostsFilePath).ToList();
            var entry = $"{ip}\t{domain}";
            var startIdx = lines.FindIndex(l => l.Trim() == SectionStart);
            var endIdx = lines.FindIndex(l => l.Trim() == SectionEnd);

            if (startIdx == -1 || endIdx == -1)
            {
                lines.Add(string.Empty);
                lines.Add(SectionStart);
                lines.Add(entry);
                lines.Add(SectionEnd);
            }
            else
            {
                var exists = GetManagedLines(lines).Any(l =>
                    l.Trim().Equals(entry, StringComparison.OrdinalIgnoreCase));
                if (!exists)
                {
                    lines.Insert(endIdx, entry);
                }
            }

            File.WriteAllLines(HostsFilePath, lines);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to write hosts file: {ex.Message}");
            throw new InvalidOperationException($"Cannot modify hosts file. Run as Administrator. {ex.Message}");
        }
    }

    public void RemoveDomain(string domain)
    {
        try
        {
            var lines = File.ReadAllLines(HostsFilePath).ToList();
            var startIdx = lines.FindIndex(l => l.Trim() == SectionStart);
            var endIdx = lines.FindIndex(l => l.Trim() == SectionEnd);

            if (startIdx == -1 || endIdx == -1) return;

            var before = lines.Take(startIdx + 1).ToList();
            var section = lines.Skip(startIdx + 1).Take(endIdx - startIdx - 1).ToList();
            var after = lines.Skip(endIdx).ToList();

            section.RemoveAll(l => l.Contains(domain, StringComparison.OrdinalIgnoreCase) && !l.TrimStart().StartsWith("#"));

            lines.Clear();
            lines.AddRange(before);
            lines.AddRange(section);
            lines.AddRange(after);

            if (section.Count == 0)
            {
                lines.RemoveAll(l => l.Trim() == SectionStart || l.Trim() == SectionEnd);
            }

            File.WriteAllLines(HostsFilePath, lines);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to remove from hosts file: {ex.Message}");
            throw new InvalidOperationException($"Cannot modify hosts file. Run as Administrator. {ex.Message}");
        }
    }

    public List<string> GetManagedDomains()
    {
        var domains = new List<string>();
        try
        {
            var lines = File.ReadAllLines(HostsFilePath).ToList();
            foreach (var line in GetManagedLines(lines))
            {
                var parts = line.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    domains.Add(parts[1]);
            }
        }
        catch (Exception ex) { Logger.Log("HostsFileService.GetManagedDomains", ex); }
        return domains;
    }

    private static IEnumerable<string> GetManagedLines(List<string> lines)
    {
        var startIdx = lines.FindIndex(l => l.Trim() == SectionStart);
        var endIdx = lines.FindIndex(l => l.Trim() == SectionEnd);
        if (startIdx == -1 || endIdx == -1) yield break;

        for (var i = startIdx + 1; i < endIdx; i++)
        {
            var line = lines[i];
            if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("#"))
                yield return line;
        }
    }

    public bool NeedsAdmin => true;
}

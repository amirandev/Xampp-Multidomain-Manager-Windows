using System.Reflection;

namespace XamppMultidomainManager.Services;

public static class IconHelper
{
    public static string GetIconPath(string relativePath)
    {
        var baseDir = AppContext.BaseDirectory;
        var fullPath = Path.Combine(baseDir, relativePath);
        if (File.Exists(fullPath))
            return fullPath;

        var extracted = Path.Combine(Path.GetTempPath(), "XamppMultidomainManager", relativePath);
        if (File.Exists(extracted))
            return extracted;

        var dir = Path.GetDirectoryName(extracted);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var resourceName = $"XamppMultidomainManager.{relativePath.Replace('\\', '.').Replace('/', '.')}";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var fileStream = File.Create(extracted);
            stream.CopyTo(fileStream);
            return extracted;
        }

        return fullPath;
    }
}

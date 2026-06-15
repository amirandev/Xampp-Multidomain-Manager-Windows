/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
namespace XamppMultidomainManager.Services;

public static class Logger
{
    private static readonly string LogPath;
    private static readonly object Lock = new();

    static Logger()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XamppMultidomainManager");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        LogPath = Path.Combine(dir, "error.log");
    }

    public static void Log(string message)
    {
        try
        {
            lock (Lock)
            {
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
        }
        catch { }
    }

    public static void Log(string context, Exception ex)
    {
        Log($"{context}: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
    }

    public static void Log(Exception ex) =>
        Log($"Unhandled: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
}

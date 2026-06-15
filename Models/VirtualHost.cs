/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
namespace XamppMultidomainManager.Models;

public class VirtualHost
{
    public int Id { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public string DocumentRoot { get; set; } = string.Empty;
    public string? ServerAlias { get; set; }
    public bool Enabled { get; set; } = true;
    public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    public string UpdatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
}

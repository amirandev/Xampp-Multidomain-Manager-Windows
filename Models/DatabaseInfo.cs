/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
namespace XamppMultidomainManager.Models;

public class DatabaseInfo
{
    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = "0 KB";
    public int Tables { get; set; }
    public string Collation { get; set; } = "utf8_general_ci";
}

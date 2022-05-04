using System;
using System.IO;
using Newtonsoft.Json;

namespace CriminalCodeSystem;

public class UserConfig
{
    public static Dictionary<string, string> Credentials { get; set; } = new Dictionary<string, string> {
        { "MASTER_JWT_SECRET", "" },
        { "MASTER_ACCESS_TOKEN", "" },
        { "MASTER_KDF_SALT", "" },
        { "MYSQL_DB_CONNECTION", "" },
    };

    static UserConfig()
    {
        if (!File.Exists("credentials.json")) return;
        var textCredentials = File.ReadAllText("credentials.json");
        var dictCredentials = JsonConvert.DeserializeObject<Dictionary<string, string>>(textCredentials);
        foreach (var pair in dictCredentials)
        {
            Credentials[pair.Key] = pair.Value;
        }
    }
}
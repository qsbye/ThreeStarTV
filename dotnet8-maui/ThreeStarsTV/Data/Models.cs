using System.Text.Json.Serialization;

namespace ThreeStarsTV.Data;

// JSON 字段名与桌面版/Android 版保持一致，便于配置互导

public class CameraItem
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("ip")] public string Ip { get; set; } = "";
    [JsonPropertyName("remark")] public string Remark { get; set; } = "";
    [JsonPropertyName("locked")] public bool Locked { get; set; }
}

public class CameraConfig
{
    [JsonPropertyName("count")] public int Count { get; set; } = 1;
    [JsonPropertyName("delay")] public int Delay { get; set; } = 10;
    [JsonPropertyName("items")] public List<CameraItem> Items { get; set; } = new();
}

public class AppConfig
{
    [JsonPropertyName("language")] public string Language { get; set; } = "zh";
    [JsonPropertyName("theme")] public string Theme { get; set; } = "light";
    [JsonPropertyName("autoUnmute")] public bool AutoUnmute { get; set; } = true;
    [JsonPropertyName("topMost")] public bool TopMost { get; set; }        // Android: 屏幕常亮
    [JsonPropertyName("startMaximized")] public bool StartMaximized { get; set; } = true;
    [JsonPropertyName("autoStart")] public bool AutoStart { get; set; }
    [JsonPropertyName("gridFullscreen")] public bool GridFullscreen { get; set; }
}

public class UrlFavorite
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";

    public UrlFavorite() { }
    public UrlFavorite(string name, string url) { Name = name; Url = url; }
}

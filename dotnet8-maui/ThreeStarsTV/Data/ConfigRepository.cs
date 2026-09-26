using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ThreeStarsTV.Data;

/// <summary>
/// JSON 配置持久化，对标桌面版 ConfigService / Android 版 ConfigRepository：
/// CameraConfig.json / AppConfig.json / UrlFavorites.json 存放于应用私有目录。
/// </summary>
public class ConfigRepository : INotifyPropertyChanged
{
    public const string DefaultStreamUrl = "https://ldncctvwbcdbd.a.bdydns.com/ldncctvwbcd/cdrmldcctv1_1/index.m3u8";
    public const string FallbackWebUrl = "https://tv.cctv.com/live/cctv1/";
    public const string DefaultWebUrl = "https://tv.cctv.com/live/cctv5/";

    private const string CameraFile = "CameraConfig.json";
    private const string AppFile = "AppConfig.json";
    private const string FavFile = "UrlFavorites.json";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly string _dir;
    private CameraConfig _camera;
    private AppConfig _app;
    private List<UrlFavorite> _favorites;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Raise([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public ConfigRepository()
    {
        _dir = FileSystem.AppDataDirectory;
        _camera = LoadCamera();
        _app = LoadApp();
        _favorites = LoadFavorites();
    }

    public CameraConfig Camera
    {
        get => _camera;
        private set { _camera = value; Raise(); }
    }

    public AppConfig App
    {
        get => _app;
        private set { _app = value; Raise(); }
    }

    public List<UrlFavorite> Favorites
    {
        get => _favorites;
        private set { _favorites = value; Raise(); }
    }

    public CameraItem EnsureItem(int id)
    {
        var existing = Camera.Items.FirstOrDefault(i => i.Id == id);
        if (existing != null) return existing;
        var created = new CameraItem { Id = id };
        Camera.Items.Add(created);
        return created;
    }

    public void UpdateCamera(Action<CameraConfig> transform)
    {
        // 深拷贝快照，触发 PropertyChanged 让 UI 重组
        var snapshot = new CameraConfig
        {
            Count = Camera.Count,
            Delay = Camera.Delay,
            Items = Camera.Items.Select(i => new CameraItem { Id = i.Id, Ip = i.Ip, Remark = i.Remark, Locked = i.Locked }).ToList(),
        };
        transform(snapshot);
        snapshot.Count = Math.Clamp(snapshot.Count, 1, 16);
        snapshot.Delay = Math.Max(snapshot.Delay, 0);
        Camera = snapshot;
        Save(CameraFile, snapshot);
    }

    public void UpdateApp(Action<AppConfig> transform)
    {
        var snapshot = new AppConfig
        {
            Language = App.Language,
            Theme = App.Theme,
            AutoUnmute = App.AutoUnmute,
            TopMost = App.TopMost,
            StartMaximized = App.StartMaximized,
            AutoStart = App.AutoStart,
            GridFullscreen = App.GridFullscreen,
        };
        transform(snapshot);
        if (snapshot.Language != "en") snapshot.Language = "zh";
        if (snapshot.Theme != "dark") snapshot.Theme = "light";
        App = snapshot;
        Save(AppFile, snapshot);
    }

    public void UpdateFavorites(Action<List<UrlFavorite>> transform)
    {
        var copy = Favorites.ToList();
        transform(copy);
        Favorites = copy;
        Save(FavFile, copy);
    }

    private CameraConfig LoadCamera()
    {
        var path = Path.Combine(_dir, CameraFile);
        if (!File.Exists(path))
        {
            var def = DefaultCameraConfig();
            Save(CameraFile, def);
            return def;
        }
        try
        {
            var c = JsonSerializer.Deserialize<CameraConfig>(File.ReadAllText(path)) ?? DefaultCameraConfig();
            c.Count = Math.Clamp(c.Count, 1, 16);
            c.Delay = Math.Max(c.Delay, 0);
            return c;
        }
        catch { return DefaultCameraConfig(); }
    }

    private AppConfig LoadApp()
    {
        var path = Path.Combine(_dir, AppFile);
        if (!File.Exists(path)) return new AppConfig();
        try
        {
            return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path)) ?? new AppConfig();
        }
        catch { return new AppConfig(); }
    }

    private List<UrlFavorite> LoadFavorites()
    {
        var path = Path.Combine(_dir, FavFile);
        if (!File.Exists(path))
        {
            var def = DefaultUrlFavorites();
            Save(FavFile, def);
            return def;
        }
        try
        {
            return JsonSerializer.Deserialize<List<UrlFavorite>>(File.ReadAllText(path)) ?? DefaultUrlFavorites();
        }
        catch { return DefaultUrlFavorites(); }
    }

    private void Save<T>(string name, T value)
    {
        try
        {
            File.WriteAllText(Path.Combine(_dir, name), JsonSerializer.Serialize(value, JsonOpts));
        }
        catch { }
    }

    public static CameraConfig DefaultCameraConfig() => new()
    {
        Count = 1,
        Delay = 0,
        Items = new List<CameraItem>
        {
            new() { Id = 0, Ip = DefaultWebUrl, Remark = "CCTV-5" },
        },
    };

    public static List<UrlFavorite> DefaultUrlFavorites() => new()
    {
        new("CCTV-1 直播(m3u8)", DefaultStreamUrl),
        new("CCTV-1 网页", FallbackWebUrl),
        new("CCTV-2 网页", "https://tv.cctv.com/live/cctv2/"),
        new("CCTV-3 网页", "https://tv.cctv.com/live/cctv3/"),
        new("CCTV-4 网页", "https://tv.cctv.com/live/cctv4/"),
        new("CCTV-5 网页", "https://tv.cctv.com/live/cctv5/"),
        new("CCTV-6 网页", "https://tv.cctv.com/live/cctv6/"),
        new("CCTV-7 网页", "https://tv.cctv.com/live/cctv7/"),
        new("CCTV-8 网页", "https://tv.cctv.com/live/cctv8/"),
        new("CCTV-9 网页", "https://tv.cctv.com/live/cctv9/"),
        new("CCTV-10 网页", "https://tv.cctv.com/live/cctv10/"),
        new("CCTV-11 网页", "https://tv.cctv.com/live/cctv11/"),
        new("CCTV-12 网页", "https://tv.cctv.com/live/cctv12/"),
        new("CCTV-13 网页", "https://tv.cctv.com/live/cctv13/"),
    };
}

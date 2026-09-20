using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ThreeStarTV
{
    public class CameraItem
    {
        public int id { get; set; }
        public string ip { get; set; } = "";
        public string remark { get; set; } = "";
        public bool locked { get; set; }
    }

    public class CameraConfig
    {
        public int count { get; set; } = 1;
        public int delay { get; set; } = 10;
        public List<CameraItem> items { get; set; } = new List<CameraItem>();
    }

    public class AppConfig
    {
        public string language { get; set; } = "zh";
        public string theme { get; set; } = "light";
        public bool autoUnmute { get; set; } = true;     // 启动自动取消静音（有声音播放）
        public bool topMost { get; set; } = false;       // 窗口置顶
        public bool startMaximized { get; set; } = true; // 启动时窗口最大化
        public bool autoStart { get; set; } = false;     // 开机启动（shell:startup 快捷方式）
    }

    public class JobxCameraConfig
    {
        public string name { get; set; } = "";
        public string ip { get; set; } = "";
        public int ftp_port { get; set; } = 21;
        public string ftp_username { get; set; } = "admin";
        public string ftp_password { get; set; } = "";
        public string backup_directory { get; set; } = "";
        public bool ftps_enabled { get; set; } = true;
        public bool trust_all_certs { get; set; } = true;
    }

    public class JobxBackupConfig
    {
        public List<JobxCameraConfig> cameras { get; set; } = new List<JobxCameraConfig>();
    }

    public class UrlFavorite
    {
        public string name { get; set; } = "";
        public string url { get; set; } = "";
    }

    public static class ConfigService
    {
        public static readonly string ConfigDir;
        public const string DefaultStreamUrl = "https://ldncctvwbcdbd.a.bdydns.com/ldncctvwbcd/cdrmldcctv1_1/index.m3u8";
        public const string FallbackWebUrl = "https://tv.cctv.com/live/cctv1/";
        public static CameraConfig Camera { get; private set; } = new CameraConfig();
        public static AppConfig App { get; private set; } = new AppConfig();
        public static JobxBackupConfig JobxBackup { get; private set; } = new JobxBackupConfig();
        public static List<UrlFavorite> UrlFavorites { get; private set; } = new List<UrlFavorite>();

        private static List<UrlFavorite> DefaultUrlFavorites() => new List<UrlFavorite>
        {
            new UrlFavorite { name = "CCTV-1 直播(m3u8)", url = DefaultStreamUrl },
            new UrlFavorite { name = "CCTV-1 网页", url = FallbackWebUrl },
            new UrlFavorite { name = "CCTV-2 网页", url = "https://tv.cctv.com/live/cctv2/" },
            new UrlFavorite { name = "CCTV-3 网页", url = "https://tv.cctv.com/live/cctv3/" },
            new UrlFavorite { name = "CCTV-4 网页", url = "https://tv.cctv.com/live/cctv4/" },
            new UrlFavorite { name = "CCTV-5 网页", url = "https://tv.cctv.com/live/cctv5/" },
            new UrlFavorite { name = "CCTV-6 网页", url = "https://tv.cctv.com/live/cctv6/" },
            new UrlFavorite { name = "CCTV-7 网页", url = "https://tv.cctv.com/live/cctv7/" },
            new UrlFavorite { name = "CCTV-8 网页", url = "https://tv.cctv.com/live/cctv8/" },
            new UrlFavorite { name = "CCTV-9 网页", url = "https://tv.cctv.com/live/cctv9/" },
            new UrlFavorite { name = "CCTV-10 网页", url = "https://tv.cctv.com/live/cctv10/" },
            new UrlFavorite { name = "CCTV-11 网页", url = "https://tv.cctv.com/live/cctv11/" },
            new UrlFavorite { name = "CCTV-12 网页", url = "https://tv.cctv.com/live/cctv12/" },
            new UrlFavorite { name = "CCTV-13 网页", url = "https://tv.cctv.com/live/cctv13/" },
        };

        public static void SaveUrlFavorites() =>
            File.WriteAllText(Path.Combine(ConfigDir, "UrlFavorites.json"), JsonSerializer.Serialize(UrlFavorites, Opts));

        private static readonly JsonSerializerOptions Opts = new JsonSerializerOptions { WriteIndented = true };

        static ConfigService()
        {
            var exeDir = AppContext.BaseDirectory;
            if (File.Exists(Path.Combine(exeDir, "portable.txt")))
                ConfigDir = Path.Combine(exeDir, "Configs");
            else
                ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ThreeStarTV");
        }

        public static void Load()
        {
            Directory.CreateDirectory(ConfigDir);

            var camPath = Path.Combine(ConfigDir, "CameraConfig.json");
            if (!File.Exists(camPath))
            {
                // 首次启动：默认播放 CCTV-1 直播流
                Camera = new CameraConfig
                {
                    count = 1,
                    delay = 0,
                    items = new List<CameraItem>
                    {
                        new CameraItem { id = 0, ip = DefaultStreamUrl, remark = "CCTV-1" }
                    }
                };
                SaveCamera();
            }
            if (File.Exists(camPath))
            {
                try
                {
                    var node = JsonNode.Parse(File.ReadAllText(camPath));
                    if (node != null)
                    {
                        var c = new CameraConfig();
                        c.count = node["count"]?.GetValue<int>() ?? 1;
                        if (c.count < 1) c.count = 1;
                        if (c.count > 16) c.count = 16;
                        c.delay = node["delay"]?.GetValue<int>() ?? 10;
                        if (c.delay < 0) c.delay = 0;
                        foreach (var n in node["items"]?.AsArray() ?? new JsonArray())
                        {
                            if (n == null) continue;
                            c.items.Add(new CameraItem
                            {
                                id = n["id"]?.GetValue<int>() ?? c.items.Count,
                                ip = n["ip"]?.GetValue<string>() ?? "",
                                remark = n["remark"]?.GetValue<string>() ?? "",
                                locked = n["locked"]?.GetValue<bool>() ?? false,
                            });
                        }
                        Camera = c;
                    }
                }
                catch { /* 损坏配置使用默认值 */ }
            }

            var appPath = Path.Combine(ConfigDir, "AppConfig.json");
            if (File.Exists(appPath))
            {
                try
                {
                    var node = JsonNode.Parse(File.ReadAllText(appPath));
                    if (node != null)
                    {
                        var a = new AppConfig();
                        a.language = node["language"]?.GetValue<string>() ?? "zh";
                        a.theme = node["theme"]?.GetValue<string>() ?? "light";
                        a.autoUnmute = node["autoUnmute"]?.GetValue<bool>() ?? true;
                        a.topMost = node["topMost"]?.GetValue<bool>() ?? false;
                        a.startMaximized = node["startMaximized"]?.GetValue<bool>() ?? true;
                        a.autoStart = node["autoStart"]?.GetValue<bool>() ?? false;
                        App = a;
                    }
                }
                catch { }
            }

            var jobxPath = Path.Combine(ConfigDir, "JobxBackupConfig.json");
            if (File.Exists(jobxPath))
            {
                try
                {
                    var node = JsonNode.Parse(File.ReadAllText(jobxPath));
                    if (node != null)
                    {
                        var j = new JobxBackupConfig();
                        foreach (var n in node["cameras"]?.AsArray() ?? new JsonArray())
                        {
                            if (n == null) continue;
                            var cam = new JobxCameraConfig();
                            cam.name = n["name"]?.GetValue<string>() ?? "";
                            cam.ip = n["ip"]?.GetValue<string>() ?? "";
                            cam.ftp_port = n["ftp_port"]?.GetValue<int>() ?? 21;
                            cam.ftp_username = n["ftp_username"]?.GetValue<string>() ?? "admin";
                            cam.ftp_password = n["ftp_password"]?.GetValue<string>() ?? "";
                            cam.backup_directory = n["backup_directory"]?.GetValue<string>() ?? "";
                            cam.ftps_enabled = n["ftps_enabled"]?.GetValue<bool>() ?? true;
                            cam.trust_all_certs = n["trust_all_certs"]?.GetValue<bool>() ?? true;
                            j.cameras.Add(cam);
                        }
                        JobxBackup = j;
                    }
                }
                catch { }
            }

            var favPath = Path.Combine(ConfigDir, "UrlFavorites.json");
            if (!File.Exists(favPath))
            {
                UrlFavorites = DefaultUrlFavorites();
                SaveUrlFavorites();
            }
            else
            {
                try
                {
                    var node = JsonNode.Parse(File.ReadAllText(favPath));
                    foreach (var n in node?.AsArray() ?? new JsonArray())
                    {
                        if (n == null) continue;
                        UrlFavorites.Add(new UrlFavorite
                        {
                            name = n["name"]?.GetValue<string>() ?? "",
                            url = n["url"]?.GetValue<string>() ?? "",
                        });
                    }
                }
                catch { }
            }
        }

        public static void SaveCamera() =>
            File.WriteAllText(Path.Combine(ConfigDir, "CameraConfig.json"), JsonSerializer.Serialize(Camera, Opts));

        public static void SaveApp() =>
            File.WriteAllText(Path.Combine(ConfigDir, "AppConfig.json"), JsonSerializer.Serialize(App, Opts));

        public static void SaveJobxBackup() =>
            File.WriteAllText(Path.Combine(ConfigDir, "JobxBackupConfig.json"), JsonSerializer.Serialize(JobxBackup, Opts));

        public static CameraItem GetItem(int id)
        {
            return Camera.items.Find(i => i.id == id);
        }

        public static CameraItem EnsureItem(int id)
        {
            var item = GetItem(id);
            if (item == null)
            {
                item = new CameraItem { id = id };
                Camera.items.Add(item);
            }
            return item;
        }
    }
}

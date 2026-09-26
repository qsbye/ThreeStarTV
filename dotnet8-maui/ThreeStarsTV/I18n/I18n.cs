using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ThreeStarsTV.I18n;

/// <summary>对标桌面版 I18n.cs：运行时切换中英文，独立于系统语言。</summary>
public sealed class I18n : INotifyPropertyChanged
{
    private static readonly Lazy<I18n> _inst = new(() => new I18n());
    public static I18n Instance => _inst.Value;

    private string _language = "zh";
    public string Language
    {
        get => _language;
        set
        {
            var v = value == "en" ? "en" : "zh";
            if (_language == v) return;
            _language = v;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
            // 触发索引器刷新，所有绑定 T[xxx] 的 UI 自动更新
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static readonly Dictionary<string, (string Zh, string En)> Map = new()
    {
        ["appTitle"] = ("ThreeStarTV", "ThreeStarTV"),
        ["setting"] = ("设置", "Settings"),
        ["systemSettings"] = ("系统设置", "System Settings"),
        ["cameraDisplay"] = ("相机显示", "Camera Display"),
        ["cameraSettings"] = ("相机设置", "Camera Settings"),
        ["displaySettings"] = ("显示设置", "Display Settings"),
        ["appSettings"] = ("软件设置", "App Settings"),
        ["count"] = ("数量：", "Count:"),
        ["delay"] = ("错峰延时（秒）：", "Stagger delay (s):"),
        ["url"] = ("URL:", "URL:"),
        ["remark"] = ("备注:", "Remark:"),
        ["remarkPlaceholder"] = ("备注...", "Remark..."),
        ["urlPlaceholder"] = ("192.168.1.10 或 http://...", "192.168.1.10 or http://..."),
        ["lock"] = ("锁定", "Lock"),
        ["unlock"] = ("解锁", "Unlock"),
        ["refresh"] = ("刷新", "Refresh"),
        ["maximize"] = ("最大化", "Maximize"),
        ["restore"] = ("还原", "Restore"),
        ["themeLight"] = ("明亮", "Light"),
        ["themeDark"] = ("暗黑", "Dark"),
        ["themeSetting"] = ("界面主题", "Theme"),
        ["languageSetting"] = ("界面语言", "Language"),
        ["languageZh"] = ("中文", "中文"),
        ["languageEn"] = ("English", "English"),
        ["systemTime"] = ("系统时间：", "System Time:"),
        ["version"] = ("版本号：", "Version:"),
        ["apply"] = ("应用", "Apply"),
        ["cancel"] = ("取消", "Cancel"),
        ["camera"] = ("相机", "Camera"),
        ["locked"] = ("锁定", "Locked"),
        ["delete"] = ("删除", "Delete"),
        ["edit"] = ("编辑", "Edit"),
        ["save"] = ("保存", "Save"),
        ["error"] = ("错误", "Error"),
        ["about"] = ("关于", "About"),
        ["aboutDesc"] = (
            "电视直播播放软件：启动即自动播放 CCTV-1 高清直播（m3u8/HLS 流，内嵌 hls.js 播放器），播放失败自动回退央视网网页播放器；支持开机启动、自动取消静音、屏幕常亮、全屏播放等傻瓜式播放设置。",
            "Live TV player: plays CCTV-1 HD stream (m3u8/HLS, embedded hls.js player) automatically on startup, falling back to the CCTV web player on failure. Supports start-on-boot, auto unmute, keep screen on and fullscreen playback."),
        ["aboutTech"] = (
            "技术栈：.NET 8 MAUI + WebView + hls.js",
            "Tech stack: .NET 8 MAUI + WebView + hls.js"),
        ["aboutRepo"] = ("项目仓库", "Repository"),
        ["aboutClose"] = ("关闭", "Close"),
        ["startupSettings"] = ("启动设置", "Startup"),
        ["autoUnmute"] = ("自动取消静音（启动即有声音）", "Auto unmute (sound on startup)"),
        ["autoStart"] = ("开机启动", "Start on boot"),
        ["topMost"] = ("保持屏幕常亮", "Keep screen on"),
        ["startMaximized"] = ("启动时全屏播放", "Start fullscreen"),
        ["loading"] = ("加载中", "Loading"),
        ["favUrls"] = ("网址收藏", "URL Favorites"),
        ["favAdd"] = ("添加网址", "Add URL"),
        ["favName"] = ("名称", "Name"),
        ["favUrl"] = ("网址", "URL"),
        ["favCopy"] = ("复制", "Copy"),
        ["copied"] = ("已复制", "Copied"),
        ["add"] = ("添加", "Add"),
        ["confirmDelete"] = ("确定删除？", "Delete this item?"),
        ["ok"] = ("确定", "OK"),
        ["emptyCellHint"] = ("在上方输入网址开始播放", "Enter a URL above to play"),
        ["urlInvalid"] = ("网址为空", "Empty URL"),
        ["fullscreen"] = ("全屏", "Fullscreen"),
        ["exitFullscreen"] = ("退出全屏", "Exit fullscreen"),
        ["gridFullscreen"] = ("网格全屏模式", "Grid fullscreen mode"),
    };

    public string T(string key)
    {
        if (!Map.TryGetValue(key, out var p)) return key;
        return _language == "en" ? p.En : p.Zh;
    }

    /// <summary>绑定索引器：{Binding [key]} 用法。</summary>
    public string this[string key] => T(key);

    public static string Tr(string key) => Instance.T(key);
}

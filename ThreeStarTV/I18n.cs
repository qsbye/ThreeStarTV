using System;
using System.Collections.Generic;

namespace ThreeStarTV
{
    public static class I18n
    {
        public static string Language = "zh";

        public static event Action LanguageChanged;

        private static readonly Dictionary<string, string[]> Map = new Dictionary<string, string[]>
        {
            ["appTitle"] = new[] { "ThreeStarTV", "ThreeStarTV" },
            ["setting"] = new[] { "设置", "Settings" },
            ["systemSettings"] = new[] { "系统设置", "System Settings" },
            ["cameraDisplay"] = new[] { "相机显示", "Camera Display" },
            ["cameraSettings"] = new[] { "相机设置", "Camera Settings" },
            ["displaySettings"] = new[] { "显示设置", "Display Settings" },
            ["appSettings"] = new[] { "软件设置", "App Settings" },
            ["count"] = new[] { "数量：", "Count:" },
            ["delay"] = new[] { "错峰延时（秒）：", "Stagger delay (s):" },
            ["saveApply"] = new[] { "保存应用", "Save & Apply" },
            ["updateView"] = new[] { "更新视图", "Update View" },
            ["url"] = new[] { "URL:", "URL:" },
            ["remark"] = new[] { "备注:", "Remark:" },
            ["remarkPlaceholder"] = new[] { "备注...", "Remark..." },
            ["urlPlaceholder"] = new[] { "192.168.1.10 或 http://...", "192.168.1.10 or http://..." },
            ["lock"] = new[] { "锁定", "Lock" },
            ["unlock"] = new[] { "解锁", "Unlock" },
            ["refresh"] = new[] { "刷新", "Refresh" },
            ["maximize"] = new[] { "最大化", "Maximize" },
            ["restore"] = new[] { "还原", "Restore" },
            ["themeLight"] = new[] { "明亮", "Light" },
            ["themeDark"] = new[] { "暗黑", "Dark" },
            ["themeSetting"] = new[] { "界面主题", "Theme" },
            ["languageSetting"] = new[] { "界面语言", "Language" },
            ["languageZh"] = new[] { "中文", "中文" },
            ["languageEn"] = new[] { "English", "English" },
            ["systemTime"] = new[] { "系统时间：", "System Time:" },
            ["version"] = new[] { "版本号：", "Version:" },
            ["showWindow"] = new[] { "显示界面", "Show Window" },
            ["exit"] = new[] { "退出", "Exit" },
            ["apply"] = new[] { "应用", "Apply" },
            ["cancel"] = new[] { "取消", "Cancel" },
            ["camera"] = new[] { "相机", "Camera" },
            ["locked"] = new[] { "锁定", "Locked" },
            ["jobxBackup"] = new[] { "JOBX备份", "JOBX Backup" },
            ["addCamera"] = new[] { "添加相机", "Add Camera" },
            ["backup"] = new[] { "备份", "Backup" },
            ["backupAll"] = new[] { "全部备份", "Backup All" },
            ["openDir"] = new[] { "打开目录", "Open Directory" },
            ["log"] = new[] { "日志", "Log" },
            ["name"] = new[] { "名称", "Name" },
            ["ip"] = new[] { "IP", "IP" },
            ["port"] = new[] { "端口", "Port" },
            ["username"] = new[] { "用户名", "Username" },
            ["password"] = new[] { "密码", "Password" },
            ["backupDir"] = new[] { "备份目录", "Backup Directory" },
            ["ftps"] = new[] { "FTPS", "FTPS" },
            ["trustCerts"] = new[] { "信任证书", "Trust Certs" },
            ["delete"] = new[] { "删除", "Delete" },
            ["edit"] = new[] { "编辑", "Edit" },
            ["jobxCameraNotFound"] = new[] { "未找到指定相机", "Camera not found" },
            ["jobxNoFiles"] = new[] { "未找到任何 jobx 文件", "No jobx files found" },
            ["jobxSuccessFmt"] = new[] { "备份成功，共下载 {0} 个文件", "Backup succeeded, downloaded {0} file(s)" },
            ["openDirFailed"] = new[] { "打开目录失败", "Failed to open directory" },
            ["jobxSelectCamera"] = new[] { "请先选择要备份的相机", "Please select a camera to back up" },
            ["select"] = new[] { "选择", "Select" },
            ["selectBackupDir"] = new[] { "选择备份目录", "Select backup directory" },
            ["error"] = new[] { "错误", "Error" },
            ["about"] = new[] { "关于", "About" },
            ["aboutDesc"] = new[] { "电视直播播放软件：启动即自动播放 CCTV-1 高清直播（m3u8/HLS 流，内嵌 hls.js 播放器），播放失败自动回退央视网网页播放器；支持开机启动、自动取消静音、窗口置顶、启动最大化等傻瓜式播放设置。", "Live TV player: plays CCTV-1 HD stream (m3u8/HLS, embedded hls.js player) automatically on startup, falling back to the CCTV web player on failure. Supports start-with-Windows, auto unmute, always-on-top and start-maximized." },
            ["aboutTech"] = new[] { "技术栈：WinForms + AntdUI (.NET 8) + WebView2 + hls.js + FluentFTP", "Tech stack: WinForms + AntdUI (.NET 8) + WebView2 + hls.js + FluentFTP" },
            ["aboutRepo"] = new[] { "项目仓库", "Repository" },
            ["aboutClose"] = new[] { "关闭", "Close" },
            ["startupSettings"] = new[] { "启动设置", "Startup" },
            ["autoUnmute"] = new[] { "自动取消静音（启动即有声音）", "Auto unmute (sound on startup)" },
            ["autoStart"] = new[] { "开机启动", "Start with Windows" },
            ["topMost"] = new[] { "窗口置顶", "Always on top" },
            ["startMaximized"] = new[] { "启动时最大化窗口", "Start maximized" },
            ["loading"] = new[] { "加载中", "Loading" },
            ["favUrls"] = new[] { "网址收藏", "URL Favorites" },
            ["favAdd"] = new[] { "添加网址", "Add URL" },
            ["favName"] = new[] { "名称", "Name" },
            ["favUrl"] = new[] { "网址", "URL" },
            ["favCopy"] = new[] { "复制", "Copy" },
        };

        public static string T(string key)
        {
            if (Map.TryGetValue(key, out var v))
                return Language == "en" ? v[1] : v[0];
            return key;
        }

        public static void SetLanguage(string lang)
        {
            if (lang != "en") lang = "zh";
            if (Language == lang) return;
            Language = lang;
            LanguageChanged?.Invoke();
        }

        public static void RaiseChanged() => LanguageChanged?.Invoke();
    }
}

import Foundation
import Combine

/// 对标 Windows I18n.cs 与 Android I18n.kt：运行时切换中英文，独立于系统语言。
@MainActor
final class I18n: ObservableObject {
    static let shared = I18n()

    @Published var language = "zh"

    private let map: [String: (String, String)] = [
        "appTitle": ("ThreeStarTV", "ThreeStarTV"),
        "setting": ("设置", "Settings"),
        "systemSettings": ("系统设置", "System Settings"),
        "cameraDisplay": ("相机显示", "Camera Display"),
        "cameraSettings": ("相机设置", "Camera Settings"),
        "displaySettings": ("显示设置", "Display Settings"),
        "appSettings": ("软件设置", "App Settings"),
        "count": ("数量：", "Count:"),
        "delay": ("错峰延时（秒）：", "Stagger delay (s):"),
        "saveApply": ("保存应用", "Save & Apply"),
        "updateView": ("更新视图", "Update View"),
        "url": ("URL:", "URL:"),
        "remark": ("备注:", "Remark:"),
        "remarkPlaceholder": ("备注...", "Remark..."),
        "urlPlaceholder": ("192.168.1.10 或 http://...", "192.168.1.10 or http://..."),
        "lock": ("锁定", "Lock"),
        "unlock": ("解锁", "Unlock"),
        "refresh": ("刷新", "Refresh"),
        "maximize": ("最大化", "Maximize"),
        "restore": ("还原", "Restore"),
        "themeLight": ("明亮", "Light"),
        "themeDark": ("暗黑", "Dark"),
        "themeSetting": ("界面主题", "Theme"),
        "languageSetting": ("界面语言", "Language"),
        "languageZh": ("中文", "中文"),
        "languageEn": ("English", "English"),
        "systemTime": ("系统时间：", "System Time:"),
        "version": ("版本号：", "Version:"),
        "apply": ("应用", "Apply"),
        "cancel": ("取消", "Cancel"),
        "locked": ("锁定", "Locked"),
        "name": ("名称", "Name"),
        "delete": ("删除", "Delete"),
        "edit": ("编辑", "Edit"),
        "save": ("保存", "Save"),
        "select": ("选择", "Select"),
        "error": ("错误", "Error"),
        "about": ("关于", "About"),
        "aboutDesc": (
            "电视直播播放软件：启动即自动播放 CCTV-1 高清直播（m3u8/HLS 流，AVPlayer 原生播放），播放失败自动回退央视网网页播放器；支持开机启动、自动取消静音、屏幕常亮、网格全屏模式等傻瓜式播放设置。",
            "Live TV player: plays CCTV-1 HD stream (m3u8/HLS, native AVPlayer) automatically on startup, falling back to the CCTV web player on failure. Supports start-at-login, auto unmute, keep screen on and grid fullscreen mode."
        ),
        "aboutTech": (
            "技术栈：SwiftUI + AVKit/AVFoundation (HLS) + WebKit",
            "Tech stack: SwiftUI + AVKit/AVFoundation (HLS) + WebKit"
        ),
        "aboutRepo": ("项目仓库", "Repository"),
        "aboutClose": ("关闭", "Close"),
        "startupSettings": ("启动设置", "Startup"),
        "autoUnmute": ("自动取消静音（启动即有声音）", "Auto unmute (sound on startup)"),
        "autoStart": ("开机启动", "Start at login"),
        "topMost": ("屏幕常亮（防止系统休眠）", "Keep screen on (prevent sleep)"),
        "startMaximized": ("启动时最大化窗口", "Start maximized"),
        "loading": ("加载中", "Loading"),
        "favUrls": ("网址收藏", "URL Favorites"),
        "favAdd": ("添加网址", "Add URL"),
        "favName": ("名称", "Name"),
        "favUrl": ("网址", "URL"),
        "favCopy": ("复制", "Copy"),
        "fullscreen": ("全屏", "Fullscreen"),
        "exitFullscreen": ("退出全屏", "Exit fullscreen"),
        "gridFullscreen": ("网格全屏模式", "Grid fullscreen mode"),
    ]

    func t(_ key: String) -> String {
        guard let pair = map[key] else { return key }
        return language == "en" ? pair.1 : pair.0
    }

    func setLanguage(_ lang: String) {
        language = lang == "en" ? "en" : "zh"
    }
}

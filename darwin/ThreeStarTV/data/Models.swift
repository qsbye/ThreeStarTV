import Foundation

// JSON 字段名与桌面版 (ConfigService.cs)、Android 版 (Models.kt) 保持一致，三端配置可互导。

struct CameraItem: Codable, Identifiable, Equatable {
    var id: Int
    var ip: String
    var remark: String
    var locked: Bool

    init(id: Int = 0, ip: String = "", remark: String = "", locked: Bool = false) {
        self.id = id
        self.ip = ip
        self.remark = remark
        self.locked = locked
    }
}

struct CameraConfig: Codable, Equatable {
    var count: Int = 1
    var delay: Int = 10
    var items: [CameraItem] = []
}

struct AppConfig: Codable, Equatable {
    var language: String = "zh"
    var theme: String = "light"
    var autoUnmute: Bool = true       // 启动自动取消静音（有声音播放）
    var topMost: Bool = false         // 对应"窗口置顶"：macOS 上防止系统休眠、保持屏幕常亮
    var startMaximized: Bool = true   // 启动时最大化窗口
    var autoStart: Bool = false       // 开机启动（登录项）
    var gridFullscreen: Bool = false  // 网格全屏模式（隐藏工具栏与状态栏，仅留播放网格）
}

struct UrlFavorite: Codable, Identifiable, Equatable {
    var name: String
    var url: String
    var id: String { name + url }
}

import Foundation
import Combine

/// JSON 配置持久化，对标 Windows ConfigService 与 Android ConfigRepository：
/// CameraConfig.json / AppConfig.json / UrlFavorites.json 存放于
/// ~/Library/Application Support/ThreeStarTV/。
@MainActor
final class ConfigStore: ObservableObject {

    static let defaultStreamUrl =
        "https://ldncctvwbcdbd.a.bdydns.com/ldncctvwbcd/cdrmldcctv1_1/index.m3u8"
    static let fallbackWebUrl = "https://tv.cctv.com/live/cctv1/"

    @Published var camera: CameraConfig
    @Published var app: AppConfig
    @Published var favorites: [UrlFavorite]

    private let dir: URL

    private let encoder: JSONEncoder = {
        let e = JSONEncoder()
        e.outputFormatting = [.prettyPrinted, .sortedKeys]
        return e
    }()
    private let decoder = JSONDecoder()

    init() {
        let base = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask)[0]
        dir = base.appendingPathComponent("ThreeStarTV", isDirectory: true)
        try? FileManager.default.createDirectory(at: dir, withIntermediateDirectories: true)

        camera = ConfigStore.loadCamera(dir: dir, encoder: encoder, decoder: decoder)
        app = ConfigStore.loadApp(dir: dir, decoder: decoder)
        favorites = ConfigStore.loadFavorites(dir: dir, encoder: encoder, decoder: decoder)
    }

    // MARK: - 更新（整体替换 @Published 值，保证 SwiftUI 必定收到变更）

    @discardableResult
    func ensureItem(_ id: Int) -> CameraItem {
        if let item = camera.items.first(where: { $0.id == id }) { return item }
        let created = CameraItem(id: id)
        updateCamera { $0.items.append(created) }
        return created
    }

    func updateApp(_ transform: (inout AppConfig) -> Void) {
        var cfg = app
        transform(&cfg)
        if cfg.language != "en" { cfg.language = "zh" }
        if cfg.theme != "dark" { cfg.theme = "light" }
        app = cfg
        saveApp()
    }

    func updateCamera(_ transform: (inout CameraConfig) -> Void) {
        var cfg = camera
        transform(&cfg)
        cfg.count = min(max(cfg.count, 1), 16)
        cfg.delay = max(cfg.delay, 0)
        camera = cfg
        saveCamera()
    }

    func updateFavorites(_ transform: (inout [UrlFavorite]) -> Void) {
        var list = favorites
        transform(&list)
        favorites = list
        saveFavorites()
    }

    // MARK: - 保存（UI 直接改数组后也可手动调用）

    func saveApp() { save("AppConfig.json", app) }
    func saveCamera() { save("CameraConfig.json", camera) }
    func saveFavorites() { save("UrlFavorites.json", favorites) }

    private func save<T: Encodable>(_ name: String, _ value: T) {
        guard let data = try? encoder.encode(value) else { return }
        try? data.write(to: dir.appendingPathComponent(name))
    }

    // MARK: - 加载

    private static func loadCamera(dir: URL, encoder: JSONEncoder, decoder: JSONDecoder) -> CameraConfig {
        let url = dir.appendingPathComponent("CameraConfig.json")
        guard FileManager.default.fileExists(atPath: url.path) else {
            // 首次启动：默认打开央视网 CCTV-1 网页
            let def = CameraConfig(
                count: 1,
                delay: 0,
                items: [CameraItem(id: 0, ip: fallbackWebUrl, remark: "CCTV-1")]
            )
            if let data = try? encoder.encode(def) { try? data.write(to: url) }
            return def
        }
        guard let data = try? Data(contentsOf: url),
              var cfg = try? decoder.decode(CameraConfig.self, from: data) else {
            return CameraConfig(
                count: 1,
                delay: 0,
                items: [CameraItem(id: 0, ip: fallbackWebUrl, remark: "CCTV-1")]
            ) // 损坏配置使用默认值
        }
        cfg.count = min(max(cfg.count, 1), 16)
        cfg.delay = max(cfg.delay, 0)
        return cfg
    }

    private static func loadApp(dir: URL, decoder: JSONDecoder) -> AppConfig {
        let url = dir.appendingPathComponent("AppConfig.json")
        guard FileManager.default.fileExists(atPath: url.path),
              let data = try? Data(contentsOf: url),
              let cfg = try? decoder.decode(AppConfig.self, from: data) else {
            return AppConfig()
        }
        return cfg
    }

    private static func loadFavorites(dir: URL, encoder: JSONEncoder, decoder: JSONDecoder) -> [UrlFavorite] {
        let url = dir.appendingPathComponent("UrlFavorites.json")
        guard FileManager.default.fileExists(atPath: url.path) else {
            let def = defaultUrlFavorites()
            if let data = try? encoder.encode(def) { try? data.write(to: url) }
            return def
        }
        guard let data = try? Data(contentsOf: url),
              let list = try? decoder.decode([UrlFavorite].self, from: data) else {
            return defaultUrlFavorites()
        }
        return list
    }

    static func defaultUrlFavorites() -> [UrlFavorite] {
        [
            UrlFavorite(name: "CCTV-1 直播(m3u8)", url: defaultStreamUrl),
            UrlFavorite(name: "CCTV-1 网页", url: fallbackWebUrl),
            UrlFavorite(name: "CCTV-2 网页", url: "https://tv.cctv.com/live/cctv2/"),
            UrlFavorite(name: "CCTV-3 网页", url: "https://tv.cctv.com/live/cctv3/"),
            UrlFavorite(name: "CCTV-4 网页", url: "https://tv.cctv.com/live/cctv4/"),
            UrlFavorite(name: "CCTV-5 网页", url: "https://tv.cctv.com/live/cctv5/"),
            UrlFavorite(name: "CCTV-6 网页", url: "https://tv.cctv.com/live/cctv6/"),
            UrlFavorite(name: "CCTV-7 网页", url: "https://tv.cctv.com/live/cctv7/"),
            UrlFavorite(name: "CCTV-8 网页", url: "https://tv.cctv.com/live/cctv8/"),
            UrlFavorite(name: "CCTV-9 网页", url: "https://tv.cctv.com/live/cctv9/"),
            UrlFavorite(name: "CCTV-10 网页", url: "https://tv.cctv.com/live/cctv10/"),
            UrlFavorite(name: "CCTV-11 网页", url: "https://tv.cctv.com/live/cctv11/"),
            UrlFavorite(name: "CCTV-12 网页", url: "https://tv.cctv.com/live/cctv12/"),
            UrlFavorite(name: "CCTV-13 网页", url: "https://tv.cctv.com/live/cctv13/"),
        ]
    }
}

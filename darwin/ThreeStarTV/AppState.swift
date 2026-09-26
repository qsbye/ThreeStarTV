import Foundation

/// 应用级共享状态：配置仓库 + 国际化，供各界面使用。
@MainActor
enum AppState {
    static let store = ConfigStore()
    static let i18n = I18n.shared
}

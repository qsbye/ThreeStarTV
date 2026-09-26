import SwiftUI

@main
struct ThreeStarTVApp: App {
    var body: some Scene {
        WindowGroup {
            MainScreen()
        }
        .defaultSize(width: 1200, height: 800)

        Settings {
            SettingsView()
        }
    }
}

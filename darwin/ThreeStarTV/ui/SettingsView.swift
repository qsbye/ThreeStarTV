import SwiftUI
import AppKit

/// 设置窗口：相机显示 / 相机设置 / 显示设置 / 软件设置 / 网址收藏 五个标签页。
/// 所有修改即时写入 ConfigStore 并持久化（对标 Android SettingsScreen）。
struct SettingsView: View {
    @ObservedObject private var store = AppState.store
    @ObservedObject private var i18n = AppState.i18n

    var body: some View {
        TabView {
            cameraDisplayTab
                .tabItem { Label(i18n.t("cameraDisplay"), systemImage: "rectangle.grid.2x2") }
            cameraSettingsTab
                .tabItem { Label(i18n.t("cameraSettings"), systemImage: "camera") }
            displaySettingsTab
                .tabItem { Label(i18n.t("displaySettings"), systemImage: "paintbrush") }
            appSettingsTab
                .tabItem { Label(i18n.t("appSettings"), systemImage: "wrench") }
            favoritesTab
                .tabItem { Label(i18n.t("favUrls"), systemImage: "star") }
        }
        .padding()
    }

    // MARK: - 相机显示

    private var cameraDisplayTab: some View {
        Form {
            Section {
                Stepper(value: cameraCount, in: 1...16) {
                    Text("\(i18n.t("count")) \(store.camera.count)")
                }
                Stepper(value: cameraDelay, in: 0...60) {
                    Text("\(i18n.t("delay")) \(store.camera.delay)")
                }
            }
        }
        .formStyle(.grouped)
        .frame(width: 420, height: 260)
    }

    private var cameraCount: Binding<Int> {
        Binding(
            get: { store.camera.count },
            set: { value in store.updateCamera { $0.count = value } }
        )
    }

    private var cameraDelay: Binding<Int> {
        Binding(
            get: { store.camera.delay },
            set: { value in store.updateCamera { $0.delay = value } }
        )
    }

    // MARK: - 相机设置

    private var cameraSettingsTab: some View {
        Form {
            ForEach(0..<store.camera.count, id: \.self) { index in
                Section {
                    TextField(i18n.t("urlPlaceholder"), text: itemBinding(index, keyPath: \.ip))
                        .textFieldStyle(.roundedBorder)
                        .disabled(store.camera.items.first { $0.id == index }?.locked == true)
                    TextField(i18n.t("remarkPlaceholder"), text: itemBinding(index, keyPath: \.remark))
                        .textFieldStyle(.roundedBorder)
                        .disabled(store.camera.items.first { $0.id == index }?.locked == true)
                    Toggle(i18n.t("lock"), isOn: itemBinding(index, keyPath: \.locked))
                } header: {
                    Text("#\(index + 1)")
                }
            }
        }
        .formStyle(.grouped)
        .frame(width: 460, height: 460)
    }

    private func itemBinding(_ index: Int, keyPath: WritableKeyPath<CameraItem, String>)
        -> Binding<String> {
        Binding(
            get: {
                store.ensureItem(index)[keyPath: keyPath]
            },
            set: { newValue in
                store.updateCamera { cam in
                    guard let idx = cam.items.firstIndex(where: { $0.id == index }) else {
                        var item = CameraItem(id: index)
                        item[keyPath: keyPath] = newValue
                        cam.items.append(item)
                        return
                    }
                    cam.items[idx][keyPath: keyPath] = newValue
                }
            }
        )
    }

    private func itemBinding(_ index: Int, keyPath: WritableKeyPath<CameraItem, Bool>)
        -> Binding<Bool> {
        Binding(
            get: {
                store.ensureItem(index)[keyPath: keyPath]
            },
            set: { newValue in
                store.updateCamera { cam in
                    guard let idx = cam.items.firstIndex(where: { $0.id == index }) else {
                        var item = CameraItem(id: index)
                        item[keyPath: keyPath] = newValue
                        cam.items.append(item)
                        return
                    }
                    cam.items[idx][keyPath: keyPath] = newValue
                }
            }
        )
    }

    // MARK: - 显示设置

    private var displaySettingsTab: some View {
        Form {
            Section(i18n.t("themeSetting")) {
                Picker("", selection: appBinding(\.theme)) {
                    Text(i18n.t("themeLight")).tag("light")
                    Text(i18n.t("themeDark")).tag("dark")
                }
                .pickerStyle(.radioGroup)
            }
            Section {
                Toggle(i18n.t("gridFullscreen"), isOn: appBinding(\.gridFullscreen))
            }
        }
        .formStyle(.grouped)
        .frame(width: 420, height: 260)
    }

    // MARK: - 软件设置

    private var appSettingsTab: some View {
        Form {
            Section(i18n.t("languageSetting")) {
                Picker("", selection: languageBinding) {
                    Text(i18n.t("languageZh")).tag("zh")
                    Text(i18n.t("languageEn")).tag("en")
                }
                .pickerStyle(.radioGroup)
            }
            Section(i18n.t("startupSettings")) {
                Toggle(i18n.t("autoUnmute"), isOn: appBinding(\.autoUnmute))
                Toggle(i18n.t("startMaximized"), isOn: appBinding(\.startMaximized))
                Toggle(i18n.t("autoStart"), isOn: autoStartBinding)
                Toggle(i18n.t("topMost"), isOn: appBinding(\.topMost))
            }
        }
        .formStyle(.grouped)
        .frame(width: 420, height: 360)
    }

    private func appBinding(_ keyPath: WritableKeyPath<AppConfig, Bool>) -> Binding<Bool> {
        Binding(
            get: { store.app[keyPath: keyPath] },
            set: { value in store.updateApp { $0[keyPath: keyPath] = value } }
        )
    }

    private func appBinding(_ keyPath: WritableKeyPath<AppConfig, String>) -> Binding<String> {
        Binding(
            get: { store.app[keyPath: keyPath] },
            set: { value in store.updateApp { $0[keyPath: keyPath] = value } }
        )
    }

    private var languageBinding: Binding<String> {
        Binding(
            get: { store.app.language },
            set: { value in
                store.updateApp { $0.language = value }
                i18n.setLanguage(store.app.language)
            }
        )
    }

    private var autoStartBinding: Binding<Bool> {
        Binding(
            get: { store.app.autoStart },
            set: { value in
                store.updateApp { $0.autoStart = value }
                LoginItem.setEnabled(value)
            }
        )
    }

    // MARK: - 网址收藏

    private var favoritesTab: some View {
        VStack(spacing: 0) {
            HStack {
                Spacer()
                Button {
                    store.updateFavorites {
                        $0.append(UrlFavorite(name: "New URL", url: "https://"))
                    }
                } label: {
                    Label(i18n.t("favAdd"), systemImage: "plus")
                }
            }
            .padding(.horizontal, 12)
            .padding(.vertical, 8)

            List {
                ForEach(Array(store.favorites.enumerated()), id: \.element.id) { index, favorite in
                    HStack(spacing: 8) {
                        TextField(i18n.t("favName"), text: favoriteBinding(index, keyPath: \.name))
                            .textFieldStyle(.roundedBorder)
                        TextField(i18n.t("favUrl"), text: favoriteBinding(index, keyPath: \.url))
                            .textFieldStyle(.roundedBorder)
                        Button {
                            let pasteboard = NSPasteboard.general
                            pasteboard.clearContents()
                            pasteboard.setString(favorite.url, forType: .string)
                        } label: {
                            Label(i18n.t("favCopy"), systemImage: "doc.on.doc")
                        }
                        Button(role: .destructive) {
                            store.updateFavorites { $0.removeAll { $0.id == favorite.id } }
                        } label: {
                            Image(systemName: "trash")
                        }
                    }
                    .padding(.vertical, 2)
                }
            }
        }
        .frame(width: 640, height: 440)
    }

    private func favoriteBinding(_ index: Int, keyPath: WritableKeyPath<UrlFavorite, String>)
        -> Binding<String> {
        Binding(
            get: { store.favorites[index][keyPath: keyPath] },
            set: { value in
                store.favorites[index][keyPath: keyPath] = value
                store.saveFavorites()
            }
        )
    }
}

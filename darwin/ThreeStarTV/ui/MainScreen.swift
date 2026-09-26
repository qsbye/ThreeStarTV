import SwiftUI
import AppKit

/// 主界面：顶部工具栏 + 1-16 路播放网格（支持单格最大化）+ 底部状态栏；
/// 网格全屏模式下隐藏工具栏与状态栏，仅留播放网格与右上角悬浮退出按钮。
struct MainScreen: View {
    @ObservedObject private var store = AppState.store
    @ObservedObject private var i18n = AppState.i18n

    @State private var controllers: [Int: CellController] = [:]
    @State private var maximizedId = -1
    @State private var showSettings = false
    @State private var showAbout = false
    @State private var didInitialSetup = false
    @State private var rebuildTask: Task<Void, Never>?

    /// 网格数量 → (列数, 行数)，对标 Android MainScreen.layoutMap
    private let layoutMap: [Int: (cols: Int, rows: Int)] = [
        1: (1, 1),
        2: (2, 1),
        3: (2, 2), 4: (2, 2),
        5: (3, 2), 6: (3, 2),
        7: (3, 3), 8: (3, 3), 9: (3, 3),
        10: (4, 3), 11: (4, 3), 12: (4, 3),
        13: (4, 4), 14: (4, 4), 15: (4, 4), 16: (4, 4),
    ]

    private var gridFullscreen: Bool { store.app.gridFullscreen }

    var body: some View {
        VStack(spacing: 0) {
            if !gridFullscreen {
                toolbar
                Divider()
            }
            content
            if !gridFullscreen {
                Divider()
                statusBar
            }
        }
        .frame(minWidth: 600, minHeight: 400)
        .preferredColorScheme(store.app.theme == "dark" ? .dark : .light)
        .sheet(isPresented: $showSettings) {
            SettingsView()
                .frame(minWidth: 720, minHeight: 520)
        }
        .sheet(isPresented: $showAbout) {
            AboutView()
                .frame(width: 480)
        }
        .task {
            guard !didInitialSetup else { return }
            didInitialSetup = true
            i18n.setLanguage(store.app.language)
            PowerAssertion.shared.set(enabled: store.app.topMost)
            if store.app.startMaximized { maximizeWindowOnce() }
            rebuild()
        }
        .onChange(of: store.camera.count) { _ in rebuild() }
        .onChange(of: store.camera.delay) { _ in rebuild() }
        .onChange(of: store.app.autoUnmute) { value in
            controllers.values.forEach { $0.applyAutoUnmute(value) }
        }
        .onChange(of: store.app.topMost) { value in
            PowerAssertion.shared.set(enabled: value)
        }
    }

    // MARK: - 工具栏

    private var toolbar: some View {
        HStack(spacing: 8) {
            Text(i18n.t("appTitle"))
                .font(.headline)
            Spacer()

            Button {
                showSettings = true
            } label: {
                Label(i18n.t("setting"), systemImage: "gearshape")
            }

            Button {
                let dark = store.app.theme == "dark"
                store.updateApp { $0.theme = dark ? "light" : "dark" }
            } label: {
                Label(
                    i18n.t(store.app.theme == "dark" ? "themeLight" : "themeDark"),
                    systemImage: store.app.theme == "dark" ? "sun.max" : "moon"
                )
            }

            Button {
                let toEn = i18n.language != "en"
                store.updateApp { $0.language = toEn ? "en" : "zh" }
                i18n.setLanguage(store.app.language)
            } label: {
                Text(i18n.language == "en" ? "中文" : "EN")
            }

            Button {
                showAbout = true
            } label: {
                Label(i18n.t("about"), systemImage: "info.circle")
            }

            Button {
                store.updateApp { $0.gridFullscreen = true }
            } label: {
                Label(i18n.t("fullscreen"), systemImage: "arrow.up.left.and.arrow.down.right")
            }
        }
        .padding(.horizontal, 10)
        .frame(height: 44)
    }

    // MARK: - 内容区

    @ViewBuilder
    private var content: some View {
        ZStack {
            if maximizedId >= 0, let ctrl = controllers[maximizedId] {
                PlayerCell(id: maximizedId, controller: ctrl, maximized: true) {
                    maximizedId = -1
                }
            } else {
                grid
            }
        }
        .overlay(alignment: .topTrailing) {
            if gridFullscreen {
                Button {
                    store.updateApp { $0.gridFullscreen = false }
                } label: {
                    Label(i18n.t("exitFullscreen"), systemImage: "arrow.down.right.and.arrow.up.left")
                        .labelStyle(.titleAndIcon)
                }
                .buttonStyle(.bordered)
                .padding(8)
            }
        }
    }

    @ViewBuilder
    private var grid: some View {
        let count = store.camera.count
        let layout = layoutMap[count] ?? (cols: 4, rows: 4)
        VStack(spacing: 1) {
            ForEach(0..<layout.rows, id: \.self) { row in
                HStack(spacing: 1) {
                    ForEach(0..<layout.cols, id: \.self) { col in
                        let index = row * layout.cols + col
                        if index < count, let ctrl = controllers[index] {
                            PlayerCell(
                                id: index,
                                controller: ctrl,
                                maximized: maximizedId == index
                            ) {
                                maximizedId = maximizedId == index ? -1 : index
                            }
                        } else {
                            Color(nsColor: .windowBackgroundColor)
                        }
                    }
                }
            }
        }
    }

    // MARK: - 状态栏

    private var statusBar: some View {
        HStack {
            TimelineView(.periodic(from: Date(), by: 1)) { context in
                Text(Self.timeFormatter.string(from: context.date))
                    .font(.caption)
                    .monospacedDigit()
            }
            Spacer()
            Text("\(i18n.t("version")) \(Self.appVersion)")
                .font(.caption)
                .foregroundStyle(.secondary)
        }
        .padding(.horizontal, 10)
        .frame(height: 26)
    }

    private static let timeFormatter: DateFormatter = {
        let f = DateFormatter()
        f.dateFormat = "yyyy-MM-dd HH:mm:ss"
        return f
    }()

    private static var appVersion: String {
        (Bundle.main.infoDictionary?["CFBundleShortVersionString"] as? String) ?? "1.0.0"
    }

    // MARK: - 网格重建

    private func rebuild() {
        let count = store.camera.count
        let delay = store.camera.delay

        // 超出新数量的单元格立即释放
        for (index, ctrl) in controllers where index >= count {
            ctrl.release()
            controllers[index] = nil
        }

        rebuildTask?.cancel()
        rebuildTask = Task { @MainActor in
            for index in 0..<count {
                let ctrl: CellController
                if let existing = controllers[index] {
                    ctrl = existing
                } else {
                    let created = CellController(id: index)
                    created.applyAutoUnmute(store.app.autoUnmute)
                    controllers[index] = created
                    ctrl = created
                }
                ctrl.applyAutoUnmute(store.app.autoUnmute)
                if delay > 0, index > 0 {
                    try? await Task.sleep(nanoseconds: UInt64(delay) * 1_000_000_000)
                }
                guard !Task.isCancelled else { return }
                let item = store.ensureItem(index)
                ctrl.load(item.ip)
            }
        }
    }

    private func maximizeWindowOnce() {
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.3) {
            guard let window = NSApp.windows.first(where: { $0.isVisible && $0.contentView != nil })
                ?? NSApp.mainWindow else { return }
            if let screen = window.screen, window.frame.width < screen.visibleFrame.width * 0.85 {
                window.zoom(nil)
            }
        }
    }
}

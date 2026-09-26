import SwiftUI
import AVKit
import WebKit

/// 单格播放视图：顶栏（地址/备注/锁定/刷新/最大化）+ 视频区（HLS 或网页）+ 加载遮罩。
struct PlayerCell: View {
    let id: Int
    @ObservedObject var controller: CellController
    let maximized: Bool
    let onToggleMaximize: () -> Void

    @ObservedObject private var store = AppState.store
    @ObservedObject private var i18n = AppState.i18n

    @State private var urlText = ""
    @State private var remarkText = ""
    @State private var initialized = false

    private var item: CameraItem? {
        store.camera.items.first { $0.id == id }
    }

    var body: some View {
        VStack(spacing: 0) {
            topBar
            Divider()
            videoArea
        }
        .background(Color(nsColor: .controlBackgroundColor))
        .overlay(
            Rectangle()
                .strokeBorder(Color(nsColor: .separatorColor), lineWidth: 1)
        )
        .onAppear {
            if !initialized {
                urlText = item?.ip ?? ""
                remarkText = item?.remark ?? ""
                initialized = true
            }
        }
        .onChange(of: item?.ip) { newValue in
            if urlText != newValue { urlText = newValue ?? "" }
        }
    }

    // MARK: - 顶栏

    private var topBar: some View {
        HStack(spacing: 6) {
            TextField(i18n.t("urlPlaceholder"), text: $urlText, onCommit: commitUrl)
                .textFieldStyle(.roundedBorder)
                .disabled(item?.locked == true)
                .frame(minWidth: 160)
            TextField(i18n.t("remarkPlaceholder"), text: $remarkText, onCommit: commitRemark)
                .textFieldStyle(.roundedBorder)
                .disabled(item?.locked == true)
                .frame(maxWidth: 180)

            let locked = item?.locked == true
            Button {
                store.updateCamera { cam in
                    guard let idx = cam.items.firstIndex(where: { $0.id == id }) else { return }
                    cam.items[idx].locked.toggle()
                }
            } label: {
                Text(i18n.t(locked ? "unlock" : "lock"))
            }

            Button {
                controller.reload()
            } label: {
                Label(i18n.t("refresh"), systemImage: "arrow.clockwise")
            }

            Button(action: onToggleMaximize) {
                Text(i18n.t(maximized ? "restore" : "maximize"))
            }
        }
        .padding(.horizontal, 6)
        .padding(.vertical, 4)
        .frame(height: 40)
    }

    private func commitUrl() {
        let text = urlText.trimmingCharacters(in: .whitespacesAndNewlines)
        store.updateCamera { cam in
            if let idx = cam.items.firstIndex(where: { $0.id == id }) {
                cam.items[idx].ip = text
            } else {
                cam.items.append(CameraItem(id: id, ip: text))
            }
        }
        controller.load(text)
    }

    private func commitRemark() {
        let text = remarkText.trimmingCharacters(in: .whitespacesAndNewlines)
        store.updateCamera { cam in
            if let idx = cam.items.firstIndex(where: { $0.id == id }) {
                cam.items[idx].remark = text
            } else {
                cam.items.append(CameraItem(id: id, remark: text))
            }
        }
    }

    // MARK: - 视频区

    @ViewBuilder
    private var videoArea: some View {
        ZStack {
            switch controller.mode {
            case .empty:
                Color(nsColor: .windowBackgroundColor)
            case .hls:
                if let player = controller.player {
                    VideoPlayer(player: player)
                } else {
                    Color.black
                }
            case .web:
                WebViewContainer(controller: controller)
            }
            if controller.loading {
                loadingOverlay
            }
        }
    }

    private var loadingOverlay: some View {
        ZStack {
            Color.black.opacity(0.45)
            VStack(spacing: 8) {
                ProgressView()
                    .progressViewStyle(.circular)
                    .controlSize(.large)
                Text("\(i18n.t("loading")) \(controller.loadingMs) ms")
                    .font(.callout)
                    .foregroundStyle(.white)
                ProgressView(value: Double(controller.progress), total: 100)
                    .frame(width: 220)
            }
        }
        .transition(.opacity)
    }
}

// MARK: - WKWebView 封装

struct WebViewContainer: NSViewRepresentable {
    let controller: CellController

    func makeNSView(context: Context) -> WKWebView {
        controller.ensureWebView()
    }

    func updateNSView(_ nsView: WKWebView, context: Context) {}
}

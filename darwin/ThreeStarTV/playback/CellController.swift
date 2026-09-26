import Foundation
import AVFoundation
import AVKit
import WebKit
import Combine

enum CellMode {
    case empty
    case hls
    case web
}

/// 单个播放单元格控制器，对标 Windows CameraCell 与 Android CellController：
/// - .m3u8 → AVPlayer 原生 HLS 播放
/// - 其他 http(s) → WKWebView 加载网页
/// - HLS 连续失败 3 次 → 回退央视网网页播放器
@MainActor
final class CellController: NSObject, ObservableObject {

    @Published var mode: CellMode = .empty
    @Published var loading = false
    @Published var loadingMs: Int64 = 0
    @Published var progress = 0
    @Published var playing = false
    @Published var player: AVPlayer?

    private(set) var webView: WKWebView?

    let id: Int
    private var retries = 0
    private var currentUrl = ""
    private var autoUnmute = true

    private var itemObservation: NSKeyValueObservation?
    private var timeControlObs: NSKeyValueObservation?
    private var loadingTask: Task<Void, Never>?
    private var retryTask: Task<Void, Never>?

    init(id: Int) {
        self.id = id
        super.init()
    }

    // MARK: - 对外接口

    func applyAutoUnmute(_ value: Bool) {
        autoUnmute = value
        player?.volume = value ? 1 : 0
    }

    func load(_ url: String) {
        let normalized = Self.normalize(url)
        currentUrl = normalized
        retryTask?.cancel()
        guard !normalized.isEmpty else {
            releasePlayback()
            mode = .empty
            playing = false
            return
        }
        if normalized.lowercased().hasSuffix(".m3u8") {
            playHls(normalized)
        } else {
            playWeb(normalized)
        }
    }

    func reload() {
        load(currentUrl)
    }

    /// 网格移除该单元格时释放全部播放资源
    func release() {
        loadingTask?.cancel()
        retryTask?.cancel()
        itemObservation?.invalidate()
        timeControlObs?.invalidate()
        player?.pause()
        player?.replaceCurrentItem(with: nil)
        player = nil
        webView?.stopLoading()
        webView?.navigationDelegate = nil
        webView = nil
        loading = false
        playing = false
        mode = .empty
    }

    private func releasePlayback() {
        loadingTask?.cancel()
        itemObservation?.invalidate()
        player?.pause()
        player?.replaceCurrentItem(with: nil)
        webView?.stopLoading()
    }

    // MARK: - HLS

    private func playHls(_ url: String) {
        retries = 0
        mode = .hls
        playing = false
        let p = ensurePlayer()
        let item = AVPlayerItem(url: URL(string: url)!)
        itemObservation = item.observe(\.status, options: [.initial, .new]) { item, _ in
            let status = item.status
            Task { @MainActor [weak self] in
                guard let self else { return }
                if status == .failed {
                    self.fail()
                }
            }
        }
        p.replaceCurrentItem(with: item)
        showLoading()
        p.play()
    }

    private func ensurePlayer() -> AVPlayer {
        if let p = player { return p }
        let p = AVPlayer()
        p.volume = autoUnmute ? 1 : 0
        timeControlObs = p.observe(\.timeControlStatus, options: [.initial, .new]) { player, _ in
            let isPlaying = player.timeControlStatus == .playing
            Task { @MainActor [weak self] in
                guard let self else { return }
                if isPlaying {
                    self.hideLoading()
                    self.playing = true
                }
            }
        }
        player = p
        return p
    }

    private func fail() {
        guard mode == .hls else { return }
        retries += 1
        if retries >= 3 {
            // 连续失败 3 次，回退央视网网页播放器
            playWeb(ConfigStore.fallbackWebUrl)
            return
        }
        retryTask = Task { @MainActor [weak self] in
            try? await Task.sleep(nanoseconds: 3_000_000_000)
            guard let self, !Task.isCancelled, self.mode == .hls else { return }
            let item = AVPlayerItem(url: URL(string: self.currentUrl)!)
            self.itemObservation = item.observe(\.status) { item, _ in
                let status = item.status
                Task { @MainActor [weak self] in
                    if status == .failed { self?.fail() }
                }
            }
            self.showLoading()
            self.player?.replaceCurrentItem(with: item)
            self.player?.play()
        }
    }

    // MARK: - Web

    func ensureWebView() -> WKWebView {
        if let w = webView { return w }
        let config = WKWebViewConfiguration()
        config.mediaTypesRequiringUserActionForPlayback = []
        let w = WKWebView(frame: .zero, configuration: config)
        w.allowsBackForwardNavigationGestures = true
        w.navigationDelegate = self
        webView = w
        return w
    }

    private func playWeb(_ url: String) {
        mode = .web
        playing = false
        showLoading()
        let w = ensureWebView()
        guard let u = URL(string: url) else { return }
        w.load(URLRequest(url: u))
    }

    /// 央视网回退页自动点"全屏"：仅对 cctv.com 生效，按钮渲染出来后每秒尝试一次直至成功
    private func injectAutoFullscreen() {
        let script = """
        (function(){
          if (/cctv\\.com$|\\.cctv\\.com$/.test(location.hostname)) {
            var timer = setInterval(function(){
              try {
                var els = document.querySelectorAll('[title*="全屏"], [class*="full"], [id*="full"]');
                for (var i = 0; i < els.length; i++) {
                  var b = els[i], t = (b.title || '') + ' ' + (b.className || '') + ' ' + (b.id || '');
                  if (/全屏|full/i.test(t) && b.offsetParent !== null) { b.click(); clearInterval(timer); return; }
                }
              } catch (e) {}
            }, 1000);
          }
        })();
        """
        webView?.evaluateJavaScript(script, completionHandler: nil)
    }

    // MARK: - 加载遮罩

    private func showLoading() {
        loading = true
        loadingMs = 0
        progress = 0
        loadingTask?.cancel()
        loadingTask = Task { @MainActor [weak self] in
            let start = Date()
            while !Task.isCancelled {
                try? await Task.sleep(nanoseconds: 100_000_000)
                let ms = Int64(Date().timeIntervalSince(start) * 1000)
                guard let self else { return }
                self.loadingMs = ms
                self.progress = min(90, Int(ms / 100))
                if ms > 60_000 { // 超过 60s 仍未起播，放弃遮罩避免永久遮挡
                    self.loading = false
                    break
                }
            }
        }
    }

    private func hideLoading() {
        loadingTask?.cancel()
        loading = false
    }

    // MARK: - 工具

    private static func normalize(_ url: String) -> String {
        var u = url.trimmingCharacters(in: .whitespacesAndNewlines)
        if u.isEmpty { return "" }
        if !u.lowercased().hasPrefix("http://"), !u.lowercased().hasPrefix("https://") {
            u = "http://" + u
        }
        return u
    }
}

// MARK: - WKNavigationDelegate

extension CellController: WKNavigationDelegate {
    @objc(webView:didFinishNavigation:)
    nonisolated func webView(_ webView: WKWebView, didFinish navigation: WKNavigation!) {
        Task { @MainActor in
            self.hideLoading()
            self.injectAutoFullscreen()
        }
    }

    @objc(webView:didFailNavigation:withError:)
    nonisolated func webView(
        _ webView: WKWebView,
        didFail navigation: WKNavigation!,
        withError error: Error
    ) {
        Task { @MainActor in
            self.hideLoading()
        }
    }

    @objc nonisolated func webView(
        _ webView: WKWebView,
        didFailProvisionalNavigation navigation: WKNavigation!,
        withError error: Error
    ) {
        Task { @MainActor in
            self.hideLoading()
        }
    }
}

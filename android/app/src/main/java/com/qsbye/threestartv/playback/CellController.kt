package com.qsbye.threestartv.playback

import android.annotation.SuppressLint
import android.content.Context
import android.webkit.WebView
import android.webkit.WebViewClient
import androidx.media3.common.MediaItem
import androidx.media3.common.PlaybackException
import androidx.media3.common.Player
import androidx.media3.exoplayer.ExoPlayer
import com.qsbye.threestartv.data.ConfigRepository
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

enum class CellMode {
  EMPTY,
  HLS,
  WEB,
}

data class CellUiState(
  val mode: CellMode = CellMode.EMPTY,
  val url: String = "",
  val loading: Boolean = false,
  val loadingMs: Long = 0,
  val loadingProgress: Int = 0, // 伪进度 0..90，起播后由遮罩隐藏收尾
  val playing: Boolean = false,
  val player: ExoPlayer? = null,
)

/**
 * 单个播放单元格控制器，对标桌面版 CameraCell：
 * - .m3u8 → ExoPlayer 原生 HLS 播放
 * - 其他 http(s) → WebView 加载网页
 * - HLS 连续失败 3 次 → 回退央视网网页播放器
 */
class CellController(
  private val context: Context,
  private val id: Int,
) {
  private val scope = CoroutineScope(Dispatchers.Main + SupervisorJob())
  private var player: ExoPlayer? = null
  private var webView: WebView? = null

  private val _ui = MutableStateFlow(CellUiState())
  val ui: StateFlow<CellUiState> = _ui

  private var retries = 0
  private var currentUrl = ""
  private var autoUnmute = true
  private var loadingJob: Job? = null
  private var retryJob: Job? = null

  private val playerListener =
    object : Player.Listener {
      override fun onPlayerError(error: PlaybackException) {
        fail()
      }

      override fun onRenderedFirstFrame() {
        hideLoading()
        _ui.update { it.copy(playing = true) }
      }

      override fun onIsPlayingChanged(isPlaying: Boolean) {
        if (isPlaying) {
          hideLoading()
          _ui.update { it.copy(playing = true) }
        }
      }
    }

  fun applyAutoUnmute(value: Boolean) {
    autoUnmute = value
    player?.volume = if (value) 1f else 0f
  }

  fun load(url: String) {
    val normalized = normalize(url)
    currentUrl = normalized
    retryJob?.cancel()
    if (normalized.isEmpty()) {
      stopPlayback()
      _ui.update { it.copy(mode = CellMode.EMPTY, url = "", loading = false, playing = false) }
      return
    }
    if (normalized.endsWith(".m3u8", ignoreCase = true)) {
      playHls(normalized)
    } else {
      playWeb(normalized)
    }
  }

  fun reload() = load(currentUrl)

  fun release() {
    loadingJob?.cancel()
    retryJob?.cancel()
    player?.removeListener(playerListener)
    player?.release()
    player = null
    webView?.destroy()
    webView = null
    _ui.update { CellUiState() }
  }

  // ---------- HLS ----------

  private fun playHls(url: String) {
    retries = 0
    _ui.update { it.copy(mode = CellMode.HLS, url = url, playing = false) }
    showLoading()
    ensurePlayer().apply {
      setMediaItem(MediaItem.fromUri(url))
      prepare()
      playWhenReady = true
    }
  }

  private fun ensurePlayer(): ExoPlayer {
    player?.let { return it }
    val p =
      ExoPlayer.Builder(context).build().apply {
        addListener(playerListener)
        volume = if (autoUnmute) 1f else 0f
      }
    player = p
    _ui.update { it.copy(player = p) }
    return p
  }

  private fun stopPlayback() {
    player?.stop()
    player?.clearMediaItems()
    webView?.stopLoading()
  }

  private fun fail() {
    if (_ui.value.mode != CellMode.HLS) return
    retries++
    if (retries >= 3) {
      // 连续失败 3 次，回退央视网网页播放器
      playWeb(ConfigRepository.FallbackWebUrl)
    } else {
      retryJob =
        scope.launch {
          delay(3000)
          val url = currentUrl
          if (url.isNotEmpty() && _ui.value.mode == CellMode.HLS) {
            showLoading()
            player?.setMediaItem(MediaItem.fromUri(url))
            player?.prepare()
            player?.playWhenReady = true
          }
        }
    }
  }

  // ---------- Web ----------

  @SuppressLint("SetJavaScriptEnabled")
  fun attachWebView(view: WebView) {
    if (webView === view) return
    webView = view
    view.settings.apply {
      javaScriptEnabled = true
      domStorageEnabled = true
      mediaPlaybackRequiresUserGesture = false // 自动播放
      mixedContentMode = android.webkit.WebSettings.MIXED_CONTENT_ALWAYS_ALLOW
    }
    view.webViewClient =
      object : WebViewClient() {
        override fun onPageFinished(view: WebView, url: String?) {
          super.onPageFinished(view, url)
          onWebPageFinished()
        }
      }
    // 首次附加或最大化还原后重挂：若当前处于网页模式，补载当前地址
    if (_ui.value.mode == CellMode.WEB && _ui.value.url.isNotEmpty()) {
      view.loadUrl(_ui.value.url)
    }
  }

  fun detachWebView(view: WebView) {
    if (webView === view) webView = null
  }

  fun onWebPageFinished() {
    if (_ui.value.mode != CellMode.WEB) return
    hideLoading()
    injectAutoFullscreen()
  }

  private fun playWeb(url: String) {
    _ui.update { it.copy(mode = CellMode.WEB, url = url, playing = false) }
    showLoading()
    webView?.loadUrl(url)
  }

  /** 央视网回退页自动点"全屏"：仅对 cctv.com 生效，按钮渲染出来后每秒尝试一次直至成功 */
  private fun injectAutoFullscreen() {
    val script =
      """
      (function(){
        if (/cctv\.com${'$'}|\.cctv\.com${'$'}/.test(location.hostname)) {
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
      """.trimIndent()
    webView?.evaluateJavascript(script, null)
  }

  // ---------- 加载遮罩 ----------

  private fun showLoading() {
    _ui.update { it.copy(loading = true, loadingMs = 0, loadingProgress = 0) }
    loadingJob?.cancel()
    loadingJob =
      scope.launch {
        val start = System.currentTimeMillis()
        while (true) {
          delay(100)
          val ms = System.currentTimeMillis() - start
          if (ms > 60000) { // 超过 60s 仍未起播，放弃遮罩避免永久遮挡
            _ui.update { it.copy(loading = false) }
            break
          }
          _ui.update { it.copy(loadingMs = ms, loadingProgress = minOf(90, (ms / 100).toInt())) }
        }
      }
  }

  private fun hideLoading() {
    loadingJob?.cancel()
    _ui.update { it.copy(loading = false) }
  }

  private fun normalize(url: String): String {
    var u = url.trim()
    if (u.isEmpty()) return ""
    if (!u.startsWith("http://", true) && !u.startsWith("https://", true)) u = "http://$u"
    return u
  }
}

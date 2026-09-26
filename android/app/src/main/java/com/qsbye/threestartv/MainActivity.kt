package com.qsbye.threestartv

import android.os.Bundle
import android.view.WindowManager
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.core.view.WindowInsetsCompat
import androidx.core.view.WindowInsetsControllerCompat
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.qsbye.threestartv.data.ConfigRepository
import com.qsbye.threestartv.i18n.I18n
import com.qsbye.threestartv.theme.Theme
import java.net.HttpURLConnection
import java.net.URL

class MainActivity : ComponentActivity() {
  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    AppState.init(this)
    I18n.setLanguage(AppState.repo.app.value.language)
    enableEdgeToEdge()
    warmupPrefetch()

    setContent {
      val app by AppState.repo.app.collectAsStateWithLifecycle()

      // "窗口置顶" → 保持屏幕常亮；"启动最大化" → 沉浸式全屏
      LaunchedEffect(app.topMost) {
        if (app.topMost) {
          window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
        } else {
          window.clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
        }
      }
      LaunchedEffect(app.startMaximized || app.gridFullscreen) {
        val controller = WindowInsetsControllerCompat(window, window.decorView)
        if (app.startMaximized || app.gridFullscreen) {
          controller.hide(WindowInsetsCompat.Type.systemBars())
          controller.systemBarsBehavior =
            WindowInsetsControllerCompat.BEHAVIOR_SHOW_TRANSIENT_BARS_BY_SWIPE
        } else {
          controller.show(WindowInsetsCompat.Type.systemBars())
        }
      }

      Theme(darkTheme = app.theme == "dark") {
        Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
          MainNavigation()
        }
      }
    }
  }

  /** 启动后台预热：提前请求直播流清单与央视网回退页，完成 DNS 解析与 TLS 握手，缩短首个画面等待。 */
  private fun warmupPrefetch() {
    Thread {
      for (url in listOf(ConfigRepository.DefaultStreamUrl, ConfigRepository.FallbackWebUrl)) {
        try {
          val conn = URL(url).openConnection() as HttpURLConnection
          conn.connectTimeout = 5000
          conn.readTimeout = 5000
          conn.setRequestProperty("User-Agent", "Mozilla/5.0")
          conn.connect()
          conn.inputStream.use { it.readBytes() }
          conn.disconnect()
        } catch (_: Exception) {
          // 预热失败不影响启动
        }
      }
    }.start()
  }
}

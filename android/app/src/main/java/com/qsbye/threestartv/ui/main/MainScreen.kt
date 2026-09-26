package com.qsbye.threestartv.ui.main

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Info
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateMapOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.qsbye.threestartv.AppState
import com.qsbye.threestartv.i18n.I18n
import com.qsbye.threestartv.playback.CellController
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import kotlin.math.ceil
import kotlin.math.sqrt
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

private val Layouts: Map<Int, Pair<Int, Int>> =
  mapOf(
    1 to (1 to 1),
    2 to (2 to 1),
    4 to (2 to 2),
    6 to (3 to 2),
    9 to (3 to 3),
    12 to (4 to 3),
    16 to (4 to 4),
  )

/** 主界面，对标桌面版 MainWindow：工具栏 + 播放网格 + 状态栏。 */
@Composable
fun MainScreen(
  onOpenSettings: () -> Unit,
  onOpenAbout: () -> Unit,
  modifier: Modifier = Modifier,
) {
  val context = LocalContext.current
  val app by AppState.repo.app.collectAsStateWithLifecycle()
  val camera by AppState.repo.camera.collectAsStateWithLifecycle()
  val lang by I18n.language.collectAsStateWithLifecycle()

  val count = camera.count
  val staggerDelay = camera.delay
  val autoUnmute = app.autoUnmute
  val dark = app.theme == "dark"
  val gridFullscreen = app.gridFullscreen

  // 控制器生命周期统一由此 Effect 管理；状态Map驱动网格重组。
  // 网格只读取不创建，避免组合期创建的控制器被 Effect 释放后出现"死引用"。
  val controllers = remember { mutableStateMapOf<Int, CellController>() }
  var maximizedId by remember { mutableIntStateOf(-1) }

  DisposableEffect(Unit) {
    onDispose { controllers.values.forEach { it.release() } }
  }

  // 重建网格：数量/延时变化时重新错峰加载（对标 RebuildGrid）
  LaunchedEffect(count, staggerDelay) {
    controllers.keys.filter { it >= count }.forEach { controllers.remove(it)?.release() }
    if (maximizedId >= count) maximizedId = -1
    for (i in 0 until count) {
      val c = controllers.getOrPut(i) { CellController(context, i) }
      c.applyAutoUnmute(autoUnmute)
      val url = AppState.repo.camera.value.items.find { it.id == i }?.ip ?: ""
      val idx = i
      if (idx == 0 || staggerDelay == 0) {
        c.load(url)
      } else {
        launch {
          delay(idx * staggerDelay * 1000L)
          c.load(url)
        }
      }
    }
  }

  // 静音设置实时生效
  LaunchedEffect(autoUnmute) {
    controllers.values.forEach { it.applyAutoUnmute(autoUnmute) }
  }

  Column(modifier = modifier.fillMaxSize().background(MaterialTheme.colorScheme.background)) {
    // 工具栏（网格全屏模式下隐藏）
    if (!gridFullscreen) {
      Row(
        modifier = Modifier.fillMaxWidth().height(48.dp).background(MaterialTheme.colorScheme.surface),
        verticalAlignment = Alignment.CenterVertically,
      ) {
        Text(
          I18n.t("appTitle"),
          style = MaterialTheme.typography.titleSmall,
          color = MaterialTheme.colorScheme.onSurface,
          modifier = Modifier.padding(start = 12.dp),
        )
        Spacer(Modifier.weight(1f))
        TextButton(onClick = onOpenSettings) {
          Icon(Icons.Default.Settings, contentDescription = null, modifier = Modifier.padding(end = 2.dp))
          Text(I18n.t("setting"))
        }
        TextButton(onClick = {
          AppState.repo.updateApp { it.theme = if (dark) "light" else "dark" }
        }) { Text(if (dark) I18n.t("themeLight") else I18n.t("themeDark")) }
        TextButton(onClick = {
          val next = if (lang == "en") "zh" else "en"
          I18n.setLanguage(next)
          AppState.repo.updateApp { it.language = next }
        }) { Text(if (lang == "en") I18n.t("languageZh") else I18n.t("languageEn")) }
        TextButton(onClick = onOpenAbout) {
          Icon(Icons.Default.Info, contentDescription = null, modifier = Modifier.padding(end = 2.dp))
          Text(I18n.t("about"))
        }
        TextButton(onClick = {
          AppState.repo.updateApp { it.gridFullscreen = true }
        }) { Text(I18n.t("fullscreen")) }
      }
      HorizontalDivider(color = MaterialTheme.colorScheme.outline)
    }

    // 中部：播放网格 / 最大化单格
    Box(modifier = Modifier.fillMaxWidth().weight(1f).background(MaterialTheme.colorScheme.background)) {
      val maxId = maximizedId
      if (maxId >= 0 && maxId < count) {
        controllers[maxId]?.let { c ->
          PlayerCell(
            id = maxId,
            controller = c,
            maximized = true,
            onToggleMaximize = { maximizedId = -1 },
            modifier = Modifier.fillMaxSize().padding(2.dp),
          )
        }
      } else {
        val (cols, rows) =
          Layouts[count]
            ?: run {
              val c = ceil(sqrt(count.toDouble())).toInt()
              c to c
            }
        Column(Modifier.fillMaxSize()) {
          repeat(rows) { r ->
            Row(Modifier.fillMaxWidth().weight(1f)) {
              repeat(cols) { cIdx ->
                val idx = r * cols + cIdx
                if (idx < count) {
                  controllers[idx]?.let { cellController ->
                    PlayerCell(
                      id = idx,
                      controller = cellController,
                      maximized = false,
                      onToggleMaximize = { maximizedId = idx },
                      modifier = Modifier.weight(1f).padding(2.dp).fillMaxSize(),
                    )
                  } ?: Spacer(Modifier.weight(1f))
                } else {
                  Spacer(Modifier.weight(1f))
                }
              }
            }
          }
        }
      }
      // 网格全屏模式：悬浮退出按钮（半透明显示于视频上方）
      if (gridFullscreen) {
        Box(
          modifier =
            Modifier.align(Alignment.TopEnd)
              .padding(6.dp)
              .background(
                MaterialTheme.colorScheme.surface.copy(alpha = 0.6f),
                RoundedCornerShape(6.dp),
              )
        ) {
          TextButton(onClick = { AppState.repo.updateApp { it.gridFullscreen = false } }) {
            Text(I18n.t("exitFullscreen"), style = MaterialTheme.typography.labelMedium)
          }
        }
      }
    }

    // 状态栏：系统时间 + 版本号（网格全屏模式下隐藏）
    if (!gridFullscreen) {
      HorizontalDivider(color = MaterialTheme.colorScheme.outline)
      StatusBar(modifier = Modifier.fillMaxWidth().height(26.dp).background(MaterialTheme.colorScheme.surface))
    }
  }
}

@Composable
private fun StatusBar(modifier: Modifier = Modifier) {
  val context = LocalContext.current
  val lang by I18n.language.collectAsStateWithLifecycle()
  var now by remember { mutableStateOf(formatNow()) }
  LaunchedEffect(Unit) {
    while (true) {
      now = formatNow()
      delay(1000)
    }
  }
  val version = remember {
    try {
      @Suppress("DEPRECATION")
      context.packageManager.getPackageInfo(context.packageName, 0).versionName ?: ""
    } catch (_: Exception) {
      ""
    }
  }
  Row(modifier = modifier.padding(start = 8.dp), verticalAlignment = Alignment.CenterVertically) {
    Text(
      I18n.t("systemTime") + " " + now,
      style = MaterialTheme.typography.labelSmall,
      color = MaterialTheme.colorScheme.onSurfaceVariant,
    )
    Text(
      I18n.t("version") + " " + version,
      style = MaterialTheme.typography.labelSmall,
      color = MaterialTheme.colorScheme.onSurfaceVariant,
      modifier = Modifier.padding(start = 16.dp),
    )
  }
  LaunchedEffect(lang) {}
}

private fun formatNow(): String =
  SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.getDefault()).format(Date())

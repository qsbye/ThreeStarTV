package com.qsbye.threestartv.ui.main

import android.webkit.WebView
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TextField
import androidx.compose.material3.TextFieldDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.focus.onFocusChanged
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalFocusManager
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.unit.dp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.media3.ui.PlayerView
import com.qsbye.threestartv.AppState
import com.qsbye.threestartv.i18n.I18n
import com.qsbye.threestartv.playback.CellController
import com.qsbye.threestartv.playback.CellMode

/**
 * 单个播放单元格，对标桌面版 CameraCell：
 * 顶栏（URL/备注/锁定/刷新/最大化）+ 播放区（ExoPlayer PlayerView 或 WebView）+ 加载遮罩。
 */
@Composable
fun PlayerCell(
  id: Int,
  controller: CellController,
  maximized: Boolean,
  onToggleMaximize: () -> Unit,
  modifier: Modifier = Modifier,
) {
  val item = AppState.repo.camera.value.items.find { it.id == id }
  val url = item?.ip ?: ""
  val remark = item?.remark ?: ""
  val locked = item?.locked ?: false
  val lang by I18n.language.collectAsStateWithLifecycle()
  val ui by controller.ui.collectAsStateWithLifecycle()

  Column(modifier = modifier.background(MaterialTheme.colorScheme.surface)) {
    // 顶栏
    Row(
      modifier = Modifier.fillMaxWidth().height(40.dp).padding(horizontal = 3.dp, vertical = 2.dp),
      verticalAlignment = Alignment.CenterVertically,
    ) {
      CellTextField(
        value = url,
        placeholder = I18n.t("urlPlaceholder"),
        readOnly = locked,
        onCommit = { text ->
          AppState.repo.updateCamera { cam ->
            cam.items.find { it.id == id }?.ip = text.trim()
          }
          controller.load(text.trim())
        },
        modifier = Modifier.weight(1f),
      )
      CellTextField(
        value = remark,
        placeholder = I18n.t("remarkPlaceholder"),
        readOnly = locked,
        onCommit = { text ->
          AppState.repo.updateCamera { cam ->
            cam.items.find { it.id == id }?.remark = text.trim()
          }
        },
        modifier = Modifier.weight(1f),
      )
      TextButton(onClick = {
        AppState.repo.updateCamera { cam ->
          cam.items.find { it.id == id }?.let { it.locked = !it.locked }
        }
      }) { Text(if (locked) I18n.t("unlock") else I18n.t("lock")) }
      TextButton(onClick = { controller.reload() }) {
        Icon(Icons.Default.Refresh, contentDescription = null, modifier = Modifier.padding(end = 2.dp))
        Text(I18n.t("refresh"))
      }
      TextButton(onClick = onToggleMaximize) {
        Text(I18n.t(if (maximized) "restore" else "maximize"))
      }
    }
    // 播放区
    Box(modifier = Modifier.fillMaxWidth().weight(1f).background(Color.Black)) {
      when (ui.mode) {
        CellMode.HLS -> {
          val player = ui.player
          if (player != null) {
            AndroidView(
              factory = { ctx -> PlayerView(ctx).apply { useController = false } },
              update = { it.player = player },
              modifier = Modifier.fillMaxSize(),
            )
          }
        }
        CellMode.WEB -> {
          AndroidView(
            factory = { ctx ->
              WebView(ctx).also { controller.attachWebView(it) }
            },
            onRelease = { controller.detachWebView(it) },
            modifier = Modifier.fillMaxSize(),
          )
        }
        CellMode.EMPTY -> {
          Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
            Text(
              I18n.t("emptyCellHint"),
              color = Color(0xFF888888),
              style = MaterialTheme.typography.bodySmall,
            )
          }
        }
      }
      // 加载遮罩：显示加载进度与用时(ms)
      if (ui.loading) {
        Box(
          modifier = Modifier.fillMaxSize().background(Color(0xEB111217)),
          contentAlignment = Alignment.Center,
        ) {
          Column(horizontalAlignment = Alignment.CenterHorizontally) {
            Text(
              "${I18n.t("loading")} ${ui.loadingMs} ms",
              color = Color.White,
              style = MaterialTheme.typography.bodySmall,
            )
            LinearProgressIndicator(
              progress = { ui.loadingProgress / 100f },
              modifier = Modifier.padding(top = 6.dp).fillMaxWidth(0.6f),
            )
          }
        }
      }
    }
  }
  LaunchedEffect(lang) {} // 语言变化时触发重组以刷新文案
}

/** 单元格内紧凑输入框：失焦或按完成键时提交。 */
@Composable
private fun CellTextField(
  value: String,
  placeholder: String,
  readOnly: Boolean,
  onCommit: (String) -> Unit,
  modifier: Modifier = Modifier,
) {
  var text by remember(value) { mutableStateOf(value) }
  var hadFocus by remember { mutableStateOf(false) }
  val focusManager = LocalFocusManager.current

  TextField(
    value = text,
    onValueChange = { text = it },
    placeholder = { Text(placeholder, style = MaterialTheme.typography.bodySmall) },
    readOnly = readOnly,
    singleLine = true,
    textStyle = MaterialTheme.typography.bodySmall,
    keyboardOptions = KeyboardOptions(imeAction = ImeAction.Done),
    keyboardActions =
      KeyboardActions(
        onDone = {
          onCommit(text)
          focusManager.clearFocus()
        }
      ),
    colors =
      TextFieldDefaults.colors(
        focusedContainerColor = MaterialTheme.colorScheme.surfaceVariant,
        unfocusedContainerColor = MaterialTheme.colorScheme.surfaceVariant,
        focusedIndicatorColor = Color.Transparent,
        unfocusedIndicatorColor = Color.Transparent,
      ),
    modifier =
      modifier
        .padding(horizontal = 1.dp)
        .height(34.dp)
        .onFocusChanged { state ->
          if (state.isFocused) {
            hadFocus = true
          } else if (hadFocus) {
            hadFocus = false
            onCommit(text)
          }
        },
  )
}

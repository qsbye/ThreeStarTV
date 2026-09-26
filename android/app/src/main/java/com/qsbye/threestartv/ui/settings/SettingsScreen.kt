package com.qsbye.threestartv.ui.settings

import android.widget.Toast
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.Checkbox
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.RadioButton
import androidx.compose.material3.Surface
import androidx.compose.material3.Tab
import androidx.compose.material3.TabRow
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalClipboardManager
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.AnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.qsbye.threestartv.AppState
import com.qsbye.threestartv.data.UrlFavorite
import com.qsbye.threestartv.i18n.I18n

private val Counts = listOf(1, 2, 4, 6, 9, 12, 16)

/** 设置界面，对标桌面版 SettingsWindow 的五个标签页。 */
@Composable
fun SettingsScreen(onBack: () -> Unit, modifier: Modifier = Modifier) {
  val lang by I18n.language.collectAsStateWithLifecycle()
  var tab by remember { mutableIntStateOf(0) }

  val tabs =
    listOf(
      I18n.t("cameraDisplay"),
      I18n.t("cameraSettings"),
      I18n.t("displaySettings"),
      I18n.t("appSettings"),
      I18n.t("favUrls"),
    )

  Surface(modifier = modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
    Column(Modifier.fillMaxSize()) {
      Row(
        modifier = Modifier.fillMaxWidth().height(48.dp),
        verticalAlignment = Alignment.CenterVertically,
      ) {
        IconButton(onClick = onBack) {
          Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = null)
        }
        Text(I18n.t("systemSettings"), style = MaterialTheme.typography.titleSmall)
      }
      HorizontalDivider(color = MaterialTheme.colorScheme.outline)

      TabRow(selectedTabIndex = tab) {
        tabs.forEachIndexed { index, title ->
          Tab(
            selected = tab == index,
            onClick = { tab = index },
            text = { Text(title, maxLines = 1) },
          )
        }
      }

      when (tab) {
        0 -> DisplayPage(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(12.dp))
        1 -> CameraPage(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(12.dp))
        2 -> ThemePage(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(12.dp))
        3 -> AppPage(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(12.dp))
        4 -> FavoritesPage(Modifier.fillMaxSize().padding(12.dp))
      }
    }
  }
  LaunchedEffect(lang) {}
}

// ---------- 相机显示 ----------

@Composable
private fun DisplayPage(modifier: Modifier = Modifier) {
  val camera by AppState.repo.camera.collectAsStateWithLifecycle()
  Column(modifier) {
    Text(I18n.t("count"), style = MaterialTheme.typography.bodyMedium)
    var countMenu by remember { mutableStateOf(false) }
    Box {
      OutlinedButton(onClick = { countMenu = true }) { Text(camera.count.toString()) }
      DropdownMenu(expanded = countMenu, onDismissRequest = { countMenu = false }) {
        Counts.forEach { n ->
          DropdownMenuItem(
            text = { Text(n.toString()) },
            onClick = {
              countMenu = false
              AppState.repo.updateCamera { it.count = n }
            },
          )
        }
      }
    }

    Text(
      I18n.t("delay"),
      style = MaterialTheme.typography.bodyMedium,
      modifier = Modifier.padding(top = 16.dp),
    )
    var delayText by remember(camera.delay) { mutableStateOf(camera.delay.toString()) }
    OutlinedTextField(
      value = delayText,
      onValueChange = { input ->
        delayText = input.filter { it.isDigit() }.take(3)
        delayText.toIntOrNull()?.let { v -> AppState.repo.updateCamera { it.delay = v } }
      },
      singleLine = true,
      modifier = Modifier.width(140.dp),
    )
  }
}

// ---------- 相机设置 ----------

@Composable
private fun CameraPage(modifier: Modifier = Modifier) {
  val camera by AppState.repo.camera.collectAsStateWithLifecycle()
  Column(modifier, verticalArrangement = Arrangement.spacedBy(8.dp)) {
    for (i in 0 until camera.count) {
      val item = camera.items.find { it.id == i }
      Card(
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant),
        modifier = Modifier.fillMaxWidth(),
      ) {
        Column(Modifier.padding(10.dp)) {
          Text(
            "${I18n.t("camera")} ${i + 1}",
            style = MaterialTheme.typography.bodyMedium,
            fontWeight = FontWeight.Bold,
          )
          var url by remember(item?.ip) { mutableStateOf(item?.ip ?: "") }
          OutlinedTextField(
            value = url,
            onValueChange = { url = it },
            label = { Text(I18n.t("url")) },
            readOnly = item?.locked == true,
            singleLine = true,
            modifier = Modifier.fillMaxWidth().padding(top = 4.dp),
          )
          var remark by remember(item?.remark) { mutableStateOf(item?.remark ?: "") }
          OutlinedTextField(
            value = remark,
            onValueChange = { remark = it },
            label = { Text(I18n.t("remark")) },
            readOnly = item?.locked == true,
            singleLine = true,
            modifier = Modifier.fillMaxWidth().padding(top = 4.dp),
          )
          Row(
            verticalAlignment = Alignment.CenterVertically,
            modifier = Modifier.padding(top = 2.dp),
          ) {
            Checkbox(
              checked = item?.locked == true,
              onCheckedChange = { checked ->
                AppState.repo.updateCamera { cam ->
                  cam.items.find { it.id == i }?.locked = checked
                }
              },
            )
            Text(I18n.t("locked"), style = MaterialTheme.typography.bodyMedium)
            Spacer(Modifier.weight(1f))
            TextButton(
              onClick = {
                AppState.repo.updateCamera { cam ->
                  cam.items.find { it.id == i }?.let {
                    it.ip = url.trim()
                    it.remark = remark.trim()
                  }
                }
              }
            ) { Text(I18n.t("save")) }
          }
        }
      }
    }
  }
}

// ---------- 显示设置 ----------

@Composable
private fun ThemePage(modifier: Modifier = Modifier) {
  val app by AppState.repo.app.collectAsStateWithLifecycle()
  Column(modifier) {
    Text(I18n.t("themeSetting"), fontWeight = FontWeight.Bold)
    Row(verticalAlignment = Alignment.CenterVertically) {
      RadioButton(
        selected = app.theme == "dark",
        onClick = { AppState.repo.updateApp { it.theme = "dark" } },
      )
      Text(I18n.t("themeDark"))
    }
    Row(verticalAlignment = Alignment.CenterVertically) {
      RadioButton(
        selected = app.theme != "dark",
        onClick = { AppState.repo.updateApp { it.theme = "light" } },
      )
      Text(I18n.t("themeLight"))
    }

    Row(
      verticalAlignment = Alignment.CenterVertically,
      modifier = Modifier.padding(top = 16.dp),
    ) {
      Checkbox(
        checked = app.gridFullscreen,
        onCheckedChange = { AppState.repo.updateApp { it.gridFullscreen = !it.gridFullscreen } },
      )
      Text(I18n.t("gridFullscreen"), style = MaterialTheme.typography.bodyMedium)
    }
  }
}

// ---------- 软件设置（语言 + 启动设置） ----------

@Composable
private fun AppPage(modifier: Modifier = Modifier) {
  val app by AppState.repo.app.collectAsStateWithLifecycle()
  val lang by I18n.language.collectAsStateWithLifecycle()
  Column(modifier) {
    Text(I18n.t("languageSetting"), fontWeight = FontWeight.Bold)
    Row(verticalAlignment = Alignment.CenterVertically) {
      RadioButton(selected = lang != "en", onClick = {
        I18n.setLanguage("zh")
        AppState.repo.updateApp { it.language = "zh" }
      })
      Text(I18n.t("languageZh"))
    }
    Row(verticalAlignment = Alignment.CenterVertically) {
      RadioButton(selected = lang == "en", onClick = {
        I18n.setLanguage("en")
        AppState.repo.updateApp { it.language = "en" }
      })
      Text(I18n.t("languageEn"))
    }

    Text(
      I18n.t("startupSettings"),
      fontWeight = FontWeight.Bold,
      modifier = Modifier.padding(top = 16.dp),
    )
    StartupCheckbox(I18n.t("autoUnmute"), app.autoUnmute) {
      AppState.repo.updateApp { it.autoUnmute = it.autoUnmute.not() }
    }
    StartupCheckbox(I18n.t("autoStart"), app.autoStart) {
      AppState.repo.updateApp { it.autoStart = it.autoStart.not() }
    }
    StartupCheckbox(I18n.t("topMost"), app.topMost) {
      AppState.repo.updateApp { it.topMost = it.topMost.not() }
    }
    StartupCheckbox(I18n.t("startMaximized"), app.startMaximized) {
      AppState.repo.updateApp { it.startMaximized = it.startMaximized.not() }
    }
  }
}

@Composable
private fun StartupCheckbox(label: String, checked: Boolean, onToggle: () -> Unit) {
  Row(
    verticalAlignment = Alignment.CenterVertically,
    modifier = Modifier.fillMaxWidth(),
    horizontalArrangement = Arrangement.Start,
  ) {
    Checkbox(checked = checked, onCheckedChange = { onToggle() })
    Text(label, style = MaterialTheme.typography.bodyMedium)
  }
}

// ---------- 网址收藏 ----------

@Composable
private fun FavoritesPage(modifier: Modifier = Modifier) {
  val favorites by AppState.repo.favorites.collectAsStateWithLifecycle()
  val clipboard = LocalClipboardManager.current
  val context = LocalContext.current
  var editing by remember { mutableStateOf<UrlFavorite?>(null) }

  Column(modifier) {
    Row(verticalAlignment = Alignment.CenterVertically) {
      Button(onClick = {
        AppState.repo.updateFavorites { it.add(UrlFavorite("CCTV", "")) }
      }) {
        Icon(Icons.Default.Add, contentDescription = null, modifier = Modifier.padding(end = 2.dp))
        Text(I18n.t("favAdd"))
      }
    }
    HorizontalDivider(
      color = MaterialTheme.colorScheme.outline,
      modifier = Modifier.padding(vertical = 8.dp),
    )
    val scroll = rememberScrollState()
    Column(Modifier.fillMaxSize().verticalScroll(scroll)) {
      favorites.forEachIndexed { index, fav ->
        Row(
          verticalAlignment = Alignment.CenterVertically,
          modifier = Modifier.fillMaxWidth().padding(vertical = 2.dp).clickable { editing = fav },
        ) {
          Column(Modifier.weight(1f)) {
            Text(fav.name, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Bold)
            Text(
              fav.url,
              style = MaterialTheme.typography.bodySmall,
              color = MaterialTheme.colorScheme.onSurfaceVariant,
              maxLines = 1,
            )
          }
          TextButton(onClick = {
            clipboard.setText(AnnotatedString(fav.url))
            Toast.makeText(context, I18n.t("copied"), Toast.LENGTH_SHORT).show()
          }) { Text(I18n.t("favCopy")) }
          IconButton(onClick = {
            AppState.repo.updateFavorites { list ->
              if (index in list.indices) list.removeAt(index)
            }
          }) { Icon(Icons.Default.Delete, contentDescription = I18n.t("delete")) }
        }
        HorizontalDivider(color = MaterialTheme.colorScheme.outline)
      }
    }
  }

  // 点击行编辑：以对话框形式编辑名称与网址
  editing?.let { fav ->
    val index = favorites.indexOf(fav)
    var name by remember(fav) { mutableStateOf(fav.name) }
    var url by remember(fav) { mutableStateOf(fav.url) }
    AlertDialog(
      onDismissRequest = { editing = null },
      title = { Text(I18n.t("edit")) },
      text = {
        Column {
          OutlinedTextField(
            value = name,
            onValueChange = { name = it },
            label = { Text(I18n.t("favName")) },
            singleLine = true,
          )
          OutlinedTextField(
            value = url,
            onValueChange = { url = it },
            label = { Text(I18n.t("favUrl")) },
            singleLine = true,
            modifier = Modifier.padding(top = 8.dp),
          )
        }
      },
      confirmButton = {
        TextButton(onClick = {
          if (index >= 0) {
            AppState.repo.updateFavorites { list ->
              if (index in list.indices) {
                list[index] = UrlFavorite(name.trim(), url.trim())
              }
            }
          }
          editing = null
        }) { Text(I18n.t("ok")) }
      },
      dismissButton = {
        TextButton(onClick = { editing = null }) { Text(I18n.t("cancel")) }
      },
    )
  }
}

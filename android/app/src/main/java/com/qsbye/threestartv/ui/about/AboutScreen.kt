package com.qsbye.threestartv.ui.about

import android.content.Intent
import android.net.Uri
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.Button
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.qsbye.threestartv.i18n.I18n

/** 关于界面，对标桌面版 AboutWindow。 */
@Composable
fun AboutScreen(onBack: () -> Unit, modifier: Modifier = Modifier) {
  val context = LocalContext.current
  val version = remember {
    try {
      @Suppress("DEPRECATION")
      context.packageManager.getPackageInfo(context.packageName, 0).versionName ?: ""
    } catch (_: Exception) {
      ""
    }
  }

  Surface(modifier = modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
    Column(Modifier.fillMaxSize()) {
      Row(
        modifier = Modifier.fillMaxWidth().height(48.dp),
        verticalAlignment = Alignment.CenterVertically,
      ) {
        IconButton(onClick = onBack) {
          Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = null)
        }
        Text(I18n.t("about"), style = MaterialTheme.typography.titleSmall)
      }
      HorizontalDivider(color = MaterialTheme.colorScheme.outline)

      Column(
        Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(20.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
      ) {
        Text(I18n.t("appTitle"), style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold)
        Text(
          "${I18n.t("version")} $version",
          style = MaterialTheme.typography.bodyMedium,
          color = MaterialTheme.colorScheme.onSurfaceVariant,
          modifier = Modifier.padding(top = 4.dp),
        )
        Text(
          I18n.t("aboutDesc"),
          style = MaterialTheme.typography.bodyMedium,
          modifier = Modifier.padding(top = 16.dp),
        )
        Text(
          I18n.t("aboutTech"),
          style = MaterialTheme.typography.bodySmall,
          color = MaterialTheme.colorScheme.onSurfaceVariant,
          modifier = Modifier.padding(top = 12.dp),
        )
        Spacer(Modifier.height(16.dp))
        Button(onClick = {
          val intent = Intent(Intent.ACTION_VIEW, Uri.parse("https://github.com/qsbye/ThreeStarTV"))
          context.startActivity(intent)
        }) { Text(I18n.t("aboutRepo")) }
        Spacer(Modifier.height(8.dp))
        Button(onClick = onBack) { Text(I18n.t("aboutClose")) }
      }
    }
  }
}

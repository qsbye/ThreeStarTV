package com.qsbye.threestartv

import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.navigation3.runtime.entryProvider
import androidx.navigation3.runtime.rememberNavBackStack
import androidx.navigation3.ui.NavDisplay
import com.qsbye.threestartv.ui.about.AboutScreen
import com.qsbye.threestartv.ui.main.MainScreen
import com.qsbye.threestartv.ui.settings.SettingsScreen

@Composable
fun MainNavigation() {
  val backStack = rememberNavBackStack(Main)

  NavDisplay(
    backStack = backStack,
    onBack = { backStack.removeLastOrNull() },
    entryProvider =
      entryProvider {
        entry<Main> {
          MainScreen(
            onOpenSettings = { backStack.add(Settings) },
            onOpenAbout = { backStack.add(About) },
            modifier = Modifier.fillMaxSize(),
          )
        }
        entry<Settings> { SettingsScreen(onBack = { backStack.removeLastOrNull() }) }
        entry<About> { AboutScreen(onBack = { backStack.removeLastOrNull() }) }
      },
  )
}

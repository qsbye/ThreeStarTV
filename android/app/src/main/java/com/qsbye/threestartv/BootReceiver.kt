package com.qsbye.threestartv

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent

/** 开机自启：仅在设置中开启"开机启动"时拉起主界面（对标桌面版 AutoStartService）。 */
class BootReceiver : BroadcastReceiver() {
  override fun onReceive(context: Context, intent: Intent) {
    if (intent.action != Intent.ACTION_BOOT_COMPLETED) return
    AppState.init(context)
    if (!AppState.repo.app.value.autoStart) return
    val launch =
      Intent(context, MainActivity::class.java).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
    try {
      context.startActivity(launch)
    } catch (_: Exception) {
      // Android 10+ 后台启动 Activity 受限，失败时静默忽略
    }
  }
}

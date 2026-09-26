package com.qsbye.threestartv

import android.content.Context
import com.qsbye.threestartv.data.ConfigRepository

/** 应用级单例状态：配置仓库 + 语言/主题派生状态，供各屏幕共享。 */
object AppState {
  private var initialized = false

  lateinit var repo: ConfigRepository
    private set

  fun init(context: Context) {
    if (initialized) return
    initialized = true
    repo = ConfigRepository(context.applicationContext)
  }
}

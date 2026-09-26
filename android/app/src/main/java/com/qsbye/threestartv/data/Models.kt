package com.qsbye.threestartv.data

import kotlinx.serialization.Serializable

// JSON 字段名与桌面版 ThreeStarTV (ConfigService.cs) 保持一致，便于配置互导

@Serializable
data class CameraItem(
  var id: Int = 0,
  var ip: String = "",
  var remark: String = "",
  var locked: Boolean = false,
)

@Serializable
data class CameraConfig(
  var count: Int = 1,
  var delay: Int = 10,
  var items: MutableList<CameraItem> = mutableListOf(),
)

@Serializable
data class AppConfig(
  var language: String = "zh",
  var theme: String = "light",
  var autoUnmute: Boolean = true, // 启动自动取消静音（有声音播放）
  var topMost: Boolean = false, // 对应桌面版"窗口置顶"：安卓上保持屏幕常亮
  var startMaximized: Boolean = true, // 启动时最大化窗口（沉浸式全屏）
  var autoStart: Boolean = false, // 开机启动（BOOT_COMPLETED）
  var gridFullscreen: Boolean = false, // 网格全屏模式（隐藏工具栏与状态栏，仅留播放网格）
)

@Serializable
data class UrlFavorite(
  var name: String = "",
  var url: String = "",
)

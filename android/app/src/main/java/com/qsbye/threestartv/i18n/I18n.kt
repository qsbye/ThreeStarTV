package com.qsbye.threestartv.i18n

import kotlinx.coroutines.flow.MutableStateFlow

/** 对标桌面版 I18n.cs：运行时切换中英文，独立于系统语言。 */
object I18n {
  val language = MutableStateFlow("zh")

  private val map: Map<String, Pair<String, String>> =
    mapOf(
      "appTitle" to ("ThreeStarTV" to "ThreeStarTV"),
      "setting" to ("设置" to "Settings"),
      "systemSettings" to ("系统设置" to "System Settings"),
      "cameraDisplay" to ("相机显示" to "Camera Display"),
      "cameraSettings" to ("相机设置" to "Camera Settings"),
      "displaySettings" to ("显示设置" to "Display Settings"),
      "appSettings" to ("软件设置" to "App Settings"),
      "count" to ("数量：" to "Count:"),
      "delay" to ("错峰延时（秒）：" to "Stagger delay (s):"),
      "url" to ("URL:" to "URL:"),
      "remark" to ("备注:" to "Remark:"),
      "remarkPlaceholder" to ("备注..." to "Remark..."),
      "urlPlaceholder" to ("192.168.1.10 或 http://..." to "192.168.1.10 or http://..."),
      "lock" to ("锁定" to "Lock"),
      "unlock" to ("解锁" to "Unlock"),
      "refresh" to ("刷新" to "Refresh"),
      "maximize" to ("最大化" to "Maximize"),
      "restore" to ("还原" to "Restore"),
      "themeLight" to ("明亮" to "Light"),
      "themeDark" to ("暗黑" to "Dark"),
      "themeSetting" to ("界面主题" to "Theme"),
      "languageSetting" to ("界面语言" to "Language"),
      "languageZh" to ("中文" to "中文"),
      "languageEn" to ("English" to "English"),
      "systemTime" to ("系统时间：" to "System Time:"),
      "version" to ("版本号：" to "Version:"),
      "apply" to ("应用" to "Apply"),
      "cancel" to ("取消" to "Cancel"),
      "camera" to ("相机" to "Camera"),
      "locked" to ("锁定" to "Locked"),
      "delete" to ("删除" to "Delete"),
      "edit" to ("编辑" to "Edit"),
      "save" to ("保存" to "Save"),
      "error" to ("错误" to "Error"),
      "about" to ("关于" to "About"),
      "aboutDesc" to
        (
          "电视直播播放软件：启动即自动播放 CCTV-1 高清直播（m3u8/HLS 流，原生 ExoPlayer 播放器），播放失败自动回退央视网网页播放器；支持开机启动、自动取消静音、屏幕常亮、全屏播放等傻瓜式播放设置。"
          to
            "Live TV player: plays CCTV-1 HD stream (m3u8/HLS, native ExoPlayer) automatically on startup, falling back to the CCTV web player on failure. Supports start-on-boot, auto unmute, keep screen on and fullscreen playback."
        ),
      "aboutTech" to
        (
          "技术栈：Kotlin + Jetpack Compose (Media3 ExoPlayer + WebView)"
          to "Tech stack: Kotlin + Jetpack Compose (Media3 ExoPlayer + WebView)"
        ),
      "aboutRepo" to ("项目仓库" to "Repository"),
      "aboutClose" to ("关闭" to "Close"),
      "startupSettings" to ("启动设置" to "Startup"),
      "autoUnmute" to ("自动取消静音（启动即有声音）" to "Auto unmute (sound on startup)"),
      "autoStart" to ("开机启动" to "Start on boot"),
      "topMost" to ("保持屏幕常亮" to "Keep screen on"),
      "startMaximized" to ("启动时全屏播放" to "Start fullscreen"),
      "loading" to ("加载中" to "Loading"),
      "favUrls" to ("网址收藏" to "URL Favorites"),
      "favAdd" to ("添加网址" to "Add URL"),
      "favName" to ("名称" to "Name"),
      "favUrl" to ("网址" to "URL"),
      "favCopy" to ("复制" to "Copy"),
      "copied" to ("已复制" to "Copied"),
      "add" to ("添加" to "Add"),
      "confirmDelete" to ("确定删除？" to "Delete this item?"),
      "ok" to ("确定" to "OK"),
      "emptyCellHint" to ("在上方输入网址开始播放" to "Enter a URL above to play"),
      "urlInvalid" to ("网址为空" to "Empty URL"),
      "fullscreen" to ("全屏" to "Fullscreen"),
      "exitFullscreen" to ("退出全屏" to "Exit fullscreen"),
      "gridFullscreen" to ("网格全屏模式" to "Grid fullscreen mode"),
    )

  fun t(key: String): String {
    val pair = map[key] ?: return key
    return if (language.value == "en") pair.second else pair.first
  }

  fun setLanguage(lang: String) {
    language.value = if (lang == "en") "en" else "zh"
  }
}

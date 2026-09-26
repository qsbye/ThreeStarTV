package com.qsbye.threestartv.data

import android.content.Context
import java.io.File
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.update
import kotlinx.serialization.json.Json

/**
 * JSON 配置持久化，对标桌面版 ConfigService：
 * CameraConfig.json / AppConfig.json / UrlFavorites.json 存放于应用私有目录。
 */
class ConfigRepository(context: Context) {

  companion object {
    const val DefaultStreamUrl =
      "https://ldncctvwbcdbd.a.bdydns.com/ldncctvwbcd/cdrmldcctv1_1/index.m3u8"
    const val FallbackWebUrl = "https://tv.cctv.com/live/cctv1/"

    private const val CameraFile = "CameraConfig.json"
    private const val AppFile = "AppConfig.json"
    private const val FavFile = "UrlFavorites.json"

    fun defaultCameraConfig() =
      CameraConfig(
        count = 1,
        delay = 0,
        items = mutableListOf(CameraItem(id = 0, ip = FallbackWebUrl, remark = "CCTV-1")),
      )

    fun defaultUrlFavorites() =
      mutableListOf(
        UrlFavorite("CCTV-1 直播(m3u8)", DefaultStreamUrl),
        UrlFavorite("CCTV-1 网页", FallbackWebUrl),
        UrlFavorite("CCTV-2 网页", "https://tv.cctv.com/live/cctv2/"),
        UrlFavorite("CCTV-3 网页", "https://tv.cctv.com/live/cctv3/"),
        UrlFavorite("CCTV-4 网页", "https://tv.cctv.com/live/cctv4/"),
        UrlFavorite("CCTV-5 网页", "https://tv.cctv.com/live/cctv5/"),
        UrlFavorite("CCTV-6 网页", "https://tv.cctv.com/live/cctv6/"),
        UrlFavorite("CCTV-7 网页", "https://tv.cctv.com/live/cctv7/"),
        UrlFavorite("CCTV-8 网页", "https://tv.cctv.com/live/cctv8/"),
        UrlFavorite("CCTV-9 网页", "https://tv.cctv.com/live/cctv9/"),
        UrlFavorite("CCTV-10 网页", "https://tv.cctv.com/live/cctv10/"),
        UrlFavorite("CCTV-11 网页", "https://tv.cctv.com/live/cctv11/"),
        UrlFavorite("CCTV-12 网页", "https://tv.cctv.com/live/cctv12/"),
        UrlFavorite("CCTV-13 网页", "https://tv.cctv.com/live/cctv13/"),
      )
  }

  private val json = Json { ignoreUnknownKeys = true; prettyPrint = true; encodeDefaults = true }
  private val dir: File = context.filesDir

  private val _camera = MutableStateFlow(loadCamera())
  val camera: StateFlow<CameraConfig> = _camera

  private val _app = MutableStateFlow(loadApp())
  val app: StateFlow<AppConfig> = _app

  private val _favorites = MutableStateFlow(loadFavorites())
  val favorites: StateFlow<List<UrlFavorite>> = _favorites

  fun ensureItem(id: Int): CameraItem {
    val existing = _camera.value.items.find { it.id == id }
    if (existing != null) return existing
    val created = CameraItem(id = id)
    _camera.update { it.copy(items = (it.items + created).toMutableList()) }
    return created
  }

  fun updateCamera(transform: (CameraConfig) -> Unit) {
    // 必须在快照副本（含 items 深拷贝）上修改：原地改写同一实例会让
    // MutableStateFlow 的去重判定认为值未变化而抑制发射，UI 将停留在旧状态。
    _camera.update { current ->
      val snapshot = current.copy(items = current.items.map { it.copy() }.toMutableList())
      transform(snapshot)
      snapshot.count = snapshot.count.coerceIn(1, 16)
      snapshot.delay = snapshot.delay.coerceAtLeast(0)
      snapshot
    }
    save(CameraFile, _camera.value)
  }

  fun updateApp(transform: (AppConfig) -> Unit) {
    // 同理：在 copy() 出的新实例上修改，确保 StateFlow 发射、UI 即时重组。
    _app.update { current ->
      val snapshot = current.copy()
      transform(snapshot)
      if (snapshot.language != "en") snapshot.language = "zh"
      if (snapshot.theme != "dark") snapshot.theme = "light"
      snapshot
    }
    save(AppFile, _app.value)
  }

  fun updateFavorites(transform: (MutableList<UrlFavorite>) -> Unit) {
    _favorites.update {
      val copy = it.toMutableList()
      transform(copy)
      save(FavFile, copy)
      copy
    }
  }

  private fun loadCamera(): CameraConfig {
    val f = File(dir, CameraFile)
    if (!f.exists()) {
      val def = defaultCameraConfig()
      save(CameraFile, def)
      return def
    }
    return try {
      val c = json.decodeFromString<CameraConfig>(f.readText())
      c.count = c.count.coerceIn(1, 16)
      c.delay = c.delay.coerceAtLeast(0)
      c
    } catch (_: Exception) {
      defaultCameraConfig() // 损坏配置使用默认值
    }
  }

  private fun loadApp(): AppConfig {
    val f = File(dir, AppFile)
    if (!f.exists()) return AppConfig()
    return try {
      json.decodeFromString<AppConfig>(f.readText())
    } catch (_: Exception) {
      AppConfig()
    }
  }

  private fun loadFavorites(): List<UrlFavorite> {
    val f = File(dir, FavFile)
    if (!f.exists()) {
      val def = defaultUrlFavorites()
      save(FavFile, def)
      return def
    }
    return try {
      json.decodeFromString<List<UrlFavorite>>(f.readText())
    } catch (_: Exception) {
      defaultUrlFavorites()
    }
  }

  private inline fun <reified T> save(name: String, value: T) {
    try {
      File(dir, name).writeText(json.encodeToString(value))
    } catch (_: Exception) {
    }
  }
}

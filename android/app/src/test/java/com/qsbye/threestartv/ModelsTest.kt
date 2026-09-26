package com.qsbye.threestartv

import com.qsbye.threestartv.data.AppConfig
import com.qsbye.threestartv.data.CameraConfig
import com.qsbye.threestartv.data.CameraItem
import kotlinx.serialization.json.Json
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

/** 配置 JSON 与桌面版 ThreeStarTV (ConfigService.cs) 字段名保持一致。 */
class ModelsTest {
  private val json = Json { prettyPrint = true; encodeDefaults = true }

  @Test
  fun cameraConfigKeepsDesktopFieldNames() {
    val cfg = CameraConfig(count = 4, delay = 10, items = mutableListOf(CameraItem(id = 0, ip = "http://x", remark = "CCTV-1", locked = true)))
    val text = json.encodeToString(cfg)
    assertTrue(text.contains("\"count\": 4"))
    assertTrue(text.contains("\"delay\": 10"))
    assertTrue(text.contains("\"items\""))
    assertTrue(text.contains("\"ip\": \"http://x\""))
    assertTrue(text.contains("\"locked\": true"))

    val back = json.decodeFromString<CameraConfig>(text)
    assertEquals(4, back.count)
    assertEquals("http://x", back.items[0].ip)
    assertTrue(back.items[0].locked)
  }

  @Test
  fun appConfigDefaultsMatchDesktop() {
    val def = AppConfig()
    assertEquals("zh", def.language)
    assertEquals("light", def.theme)
    assertTrue(def.autoUnmute)
    assertEquals(false, def.topMost)
    assertTrue(def.startMaximized)
    assertEquals(false, def.autoStart)
    assertEquals(false, def.gridFullscreen)
  }

  @Test
  fun ignoresUnknownKeysFromDesktopExports() {
    val withExtra = json.parseToJsonElement("{\"count\":2,\"delay\":5,\"unknownField\":1,\"items\":[]}")
    val back = Json { ignoreUnknownKeys = true }.decodeFromString<CameraConfig>(withExtra.toString())
    assertEquals(2, back.count)
  }
}

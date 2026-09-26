package com.qsbye.threestartv

import com.qsbye.threestartv.i18n.I18n
import org.junit.Assert.assertEquals
import org.junit.Test

class I18nTest {
  @Test
  fun chineseByDefault() {
    I18n.setLanguage("zh")
    assertEquals("设置", I18n.t("setting"))
  }

  @Test
  fun switchesToEnglish() {
    I18n.setLanguage("en")
    assertEquals("Settings", I18n.t("setting"))
    assertEquals("Refresh", I18n.t("refresh"))
    I18n.setLanguage("zh")
  }

  @Test
  fun unknownKeyFallsBackToKey() {
    assertEquals("noSuchKey", I18n.t("noSuchKey"))
  }

  @Test
  fun invalidLanguageFallsBackToChinese() {
    I18n.setLanguage("fr")
    assertEquals("zh", I18n.language.value)
    I18n.setLanguage("zh")
  }
}

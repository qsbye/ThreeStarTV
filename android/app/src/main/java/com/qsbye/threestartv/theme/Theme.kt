package com.qsbye.threestartv.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

// 调色板对标桌面版 ThemeManager.cs
private val DarkColorScheme =
  darkColorScheme(
    primary = Color(0xFF91CAFF),
    onPrimary = Color(0xFF0E639C),
    secondary = Color(0xFF888888),
    background = Color(0xFF1E1E1E),
    onBackground = Color(0xFFCCCCCC),
    surface = Color(0xFF252526),
    onSurface = Color(0xFFCCCCCC),
    surfaceVariant = Color(0xFF2D2D30),
    onSurfaceVariant = Color(0xFF888888),
    outline = Color(0xFF3F3F46),
  )

private val LightColorScheme =
  lightColorScheme(
    primary = Color(0xFF0078D4),
    onPrimary = Color.White,
    secondary = Color(0xFF777777),
    background = Color(0xFFF0F0F0),
    onBackground = Color(0xFF333333),
    surface = Color(0xFFFFFFFF),
    onSurface = Color(0xFF333333),
    surfaceVariant = Color(0xFFF7F7F7),
    onSurfaceVariant = Color(0xFF777777),
    outline = Color(0xFFD0D0D0),
  )

@Composable
fun Theme(darkTheme: Boolean, content: @Composable () -> Unit) {
  MaterialTheme(
    colorScheme = if (darkTheme) DarkColorScheme else LightColorScheme,
    typography = Typography,
    content = content,
  )
}

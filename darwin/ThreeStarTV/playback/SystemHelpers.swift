import Foundation
import IOKit.pwr_mgt
import ServiceManagement

/// 屏幕常亮：持有电源断言阻止系统空闲休眠与显示器休眠（对应 Windows 的"窗口置顶"）。
@MainActor
final class PowerAssertion {
    static let shared = PowerAssertion()
    private var assertionID = IOPMAssertionID(0)
    private var active = false

    func set(enabled: Bool) {
        if enabled == active { return }
        if enabled {
            let reason = "ThreeStarTV playing live TV" as CFString
            IOPMAssertionCreateWithName(
                "PreventUserIdleDisplaySleep" as CFString,
                IOPMAssertionLevel(kIOPMAssertionLevelOn),
                reason,
                &assertionID
            )
        } else {
            IOPMAssertionRelease(assertionID)
            assertionID = IOPMAssertionID(0)
        }
        active = enabled
    }
}

/// 开机启动：注册为登录项（macOS 13+）。
enum LoginItem {
    static func setEnabled(_ enabled: Bool) {
        if enabled {
            try? SMAppService.mainApp.register()
        } else {
            try? SMAppService.mainApp.unregister()
        }
    }
}

import SwiftUI
import AppKit

/// 关于窗口：软件说明、技术栈、项目仓库链接。
struct AboutView: View {
    @ObservedObject private var i18n = AppState.i18n

    private static let repoUrl = "https://gitee.com/qsBye/threestartv"

    var body: some View {
        VStack(spacing: 16) {
            Image(nsImage: NSApplication.shared.applicationIconImage)
                .resizable()
                .frame(width: 96, height: 96)

            Text(i18n.t("appTitle"))
                .font(.title.bold())

            Text(i18n.t("aboutDesc"))
                .font(.body)
                .multilineTextAlignment(.leading)
                .fixedSize(horizontal: false, vertical: true)

            Text(i18n.t("aboutTech"))
                .font(.callout)
                .foregroundStyle(.secondary)

            Link(destination: URL(string: Self.repoUrl)!) {
                Label(i18n.t("aboutRepo"), systemImage: "link")
            }

            Button(i18n.t("aboutClose")) {
                closeSheet()
            }
            .keyboardShortcut(.defaultAction)
        }
        .padding(24)
    }

    private func closeSheet() {
        NSApp.keyWindow?.close()
    }
}

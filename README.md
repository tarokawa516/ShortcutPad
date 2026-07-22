# ShortcutPad

ShortcutPadは、よく使うWindowsのキーボードショートカットを大きなボタンから1タップで実行する、常時手前表示の小型パネルです。作業中のアプリからフォーカスを奪わずにキー操作を送信します。

## このプロジェクトの経緯

Splashtopを使ってスマートフォンからWindowsへリモートアクセスすると、`Ctrl+A`、`Ctrl+C`、`Ctrl+Shift+V`などの複数キー操作は実行できても、画面上の修飾キーを組み合わせる手間がありました。

そこで「すべてのキーを再現するスクリーンキーボード」ではなく、頻度の高い操作だけを大きなボタンで簡単に実行する試作品としてShortcutPadを作りました。しばらく実用した結果、リモートアクセス時だけでなく、普段のWindows操作でも便利だと分かり、継続的に改良する独立プロジェクトへ移行しました。

## 現在できること

- 全選択してコピー: `Ctrl+A` → `Ctrl+C`
- 全選択: `Ctrl+A`
- コピー: `Ctrl+C`
- 切り取り: `Ctrl+X`
- 貼り付け: `Ctrl+V`
- アプリ固有の貼り付け: `Ctrl+Shift+V`
- 元に戻す: `Ctrl+Z`
- 検索: `Ctrl+F`
- 常に手前に表示
- パネルを操作しても対象アプリのフォーカスを維持
- 上部ドラッグによる移動、折り畳み、終了
- 高DPI表示

## 動作環境

- Windows
- .NET 10 Windows Desktop Runtime

開発にはVisual StudioのC#環境、または.NET 10 SDKを使用します。

## 実行

Visual Studioで `ShortcutPad/ShortcutPad.csproj` を開いて実行します。

コマンドラインの場合:

```powershell
dotnet run --project ShortcutPad/ShortcutPad.csproj
```

## ビルド

```powershell
dotnet build ShortcutPad/ShortcutPad.csproj
```

## 注意事項

- `Ctrl+Shift+V`の意味や対応状況は対象アプリによって異なります。
- Windowsの権限制約により、管理者権限で動作するアプリへキーを送る場合は、ShortcutPadも管理者として起動する必要があります。
- 現在はWindows専用です。

## ドキュメント

- [引き継ぎ情報](docs/HANDOFF.md)
- [設計概要](docs/ARCHITECTURE.md)
- [開発ガイド](docs/DEVELOPMENT.md)

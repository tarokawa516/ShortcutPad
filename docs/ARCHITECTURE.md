# 設計概要

## 構成

```text
ShortcutPad/
├─ Program.cs             アプリケーションのエントリーポイント
├─ ShortcutPadForm.cs     パネルの表示、ボタン、操作の定義
├─ ShortcutSender.cs      Windows SendInput APIのラッパー
├─ ShortcutPad.csproj     .NET / WinFormsプロジェクト設定
└─ app.manifest           Windows実行権限設定
```

## 入力の流れ

1. ユーザーがShortcutPadのボタンをクリックまたはタップする。
2. `WS_EX_NOACTIVATE`を設定したパネルは前面アプリのフォーカスを奪わない。
3. ボタンの処理が`ShortcutSender.SendChord`へ仮想キーと修飾キーを渡す。
4. `ShortcutSender`がキー押下・解放の`INPUT`配列を作る。
5. Windowsの`SendInput`が、フォーカスを持つアプリへ入力を送る。

## 複合操作

「全選択 → コピー」は、`Ctrl+A`を送信し、80ミリ秒待ってから`Ctrl+C`を送信します。選択状態がアプリへ反映される前にコピーが実行されるのを避けるための待ち時間です。

## Windows固有処理

- `WS_EX_NOACTIVATE`: クリックされてもウィンドウをアクティブにしない。
- `WS_EX_TOOLWINDOW`: タスク切り替え一覧などに通常のアプリとして表示しない。
- `WM_NCHITTEST` / `HTCAPTION`: ボーダーレスパネルの上部をドラッグ可能にする。
- `SendInput`: 仮想キーボード入力を送信する。

## 将来の分離候補

ボタンのカスタマイズを実装する場合は、現在`ShortcutPadForm`に直接記述している表示名・キー・修飾キー・配置を設定モデルへ分離します。設定保存を追加するまでは、現在の小さな構成を維持する方が理解しやすく安全です。

# Auto Frontline 仕様書

フロントライン向け Dalamud プラグイン。味方の密集地帯（50m 内の最多人数地点）を追跡し、vnavmesh / Rotation Solver Reborn で移動・戦闘を自動化する。

- **Dalamud API Level:** 15
- **依存:** ECommons（サブモジュール）
- **配布:** 外部リポジトリの `pluginmaster.json` + 本リポジトリ GitHub Releases（`v*` タグで CI が zip を公開）
- **マニフェスト:** `AutoFrontline/AutoFrontline.json`（`InternalName`: `AutoFrontline`。zip に `AutoFrontline.dll` / `AutoFrontline.json` を同梱）

## 必須外部プラグイン

| 表示名 | InternalName | コマンド |
|--------|--------------|---------|
| vnavmesh | `vnavmesh` | `/vnav moveto <X> <Y> <Z>` |
| Rotation Solver Reborn | `RotationSolver` | `/rotation Off` / `/rotation Auto` |

- General タブで各プラグインの有効状態を ✓ / ✗ で表示
- 両方が **インストールかつ有効（IsLoaded）** でないと **Enable** と設定スライダーは操作不可
- 不足時は設定欄の下に `Missing plugins: …` を表示
- 保存済み `Enabled=true` でも必須プラグインが外れた場合は毎フレーム `SyncEnabledState()` で自動的に `false` に戻し保存する

## 対象フィールド

| ID | 名称 |
|----|------|
| 376 | 外縁遺跡群（制圧戦） |
| 431 | シールロック（争奪戦） |
| 554 | フィールド・オブ・グローリー（砕氷戦） |
| 888 | オンサル・ハカイル（終節戦） |
| 1313 | ウォーコー・チーテ（演習戦） |

## 設定 UI

| 項目 | 既定値 | 説明 |
|------|--------|------|
| `Enabled` | false | 追従・同期の有効化 |
| `FollowIntervalSeconds` | 1 | 移動コマンド間隔（秒、UI は整数 1〜60） |
| `PlayerReselectIntervalSeconds` | 3 | 追跡対象の再選択間隔（秒、UI は整数 1〜120） |

- コマンド: `/autofrontline`
  - 引数なし: 設定を開く
  - `on` / `off` / `toggle`: `Enabled` を変更（必須プラグイン未充足時は `on` を拒否しチャットに通知）
- タブ: **General**（必須プラグイン・Enable・間隔）、**Debug**（状態表示、タブ文字色グレー）
- 設定ウィンドウ: 初回 600×600、最小 600×400。本文とフッター（バージョン・リンクボタン）を分離
- フッターリンク: GitHub（グレー背景・白文字）、OFUSE / Ko-fi（ピンク・白文字）

## 更新ループ

```
IFramework.Update
  └── RequiredPlugins.SyncEnabledState()
  └── [死亡] return
  └── FrontlineLeaveAutomation.Update()   … IsAutomationActive 時（フィールド不問）
  └── [非フロントライン] return
  └── [!IsAutomationActive] return
  └── FollowTargetService.UpdateSelection()
  └── TrackedPlayerSync.Update()
  └── [ShouldDeferMovement] return
  └── TryGetMoveTarget → MovementCommands.MoveTo
```

`IsAutomationActive` = `Enabled` かつ必須プラグイン両方ロード済み。

## メンバー収集

フロントライン内のみ。`ContentId` で重複排除。

1. GroupManager アライアンス（3×8）
2. GroupManager 自パーティ（8 + 自分）
3. InfoProxyCrossRealm

各メンバー: 名前・ContentId・EntityId・座標・死亡。EntityId 解決時は live 座標・HP を優先。

## 追跡対象

- **選定:** 生存メンバー（自分・前回追跡対象を除く）のうち、半径 **50m** 内の味方数が最大のプレイヤー（同数タイはランダム）。除外後に候補がいなければ前回対象の除外をやめて再選定
- **再選択:** 未設定時、設定間隔経過時、追跡対象死亡時（死亡は即時）

## 移動

- **移動先:** 追跡対象の位置から水平 **1m 以上 3m 未満** のランダム点
- **スキップ:** 追跡対象が前回 moveto 時から **0.1m 未満** しか動いていなければ moveto しない
- **コマンド**（移動先があるフォロー周期のみ）: `/vnav moveto <X> <Y> <Z>`

## Rotation Solver

`IsAutomationActive` かつフロントライン内・Rotation Solver ロード時。`RotationSolverState` が RSR の `DataCenter` を参照する。

| RSR 状態 | 動作 |
|---------|------|
| Off（None）または Manual | `/rotation Auto Big`（2 秒スロットルで再送） |
| Auto Big | 何もしない |
| その他 | 何もしない |

## マウント

自分が**非戦闘**（`ConditionFlag.InCombat` でない）のときは常にマウントを試行する。

| 状態 | 挙動 |
|------|------|
| 非戦闘・未マウント | 抜刀中なら `/battlemode off`（0.5s スロットル）→ マウントルーレット（1.5s スロットル） |
| 自分が戦闘中 | マウント試行しない |
| 半径 30m 内の味方が **3 人以上** 戦闘中かつ自分がマウント中 | `/mount` で降下（1.5s スロットル） |

- 味方の戦闘判定: オブジェクトが解決できる場合 `Character.InCombat`
- 非戦闘かつ未マウントのとき `ShouldDeferMovement` が true（moveto を待つ）

## ダイアログ自動操作

`IsAutomationActive` 時のみ。

| アドオン | 動作 |
|---------|------|
| `ContentsFinderConfirm` | `Commence()`（参加確定） |
| `FrontlineRecord` / `FrontlineRecord` | `Callback.Fire(addon, true, -1)` で退出要求（YesAlready と同じ） |
| `SelectYesno` | 退出確認文言または pending 中は `Yes` |

スロットル: Record 500ms、Yesno 300ms。退出要求から 60 秒で pending 解除。

## プロジェクト構成

| パス | 責務 |
|------|------|
| `Plugin.cs` | 初期化・Framework 更新登録 |
| `Configuration.cs` | 永続設定 |
| `FrontlineFields.cs` | 対象テリトリー ID / 名称 |
| `FrontlineConstants.cs` | 距離・スロットル・Node ID 等の定数 |
| `GameCoords.cs` | 座標の表示／コマンド用文字列化 |
| `Dependencies/RequiredPlugins.cs` | 必須プラグイン検証・Enable 連動 |
| `Services/AllianceMemberCollector.cs` | 味方一覧収集 |
| `Services/FollowTargetService.cs` | 追跡対象・移動先 |
| `Services/TrackedPlayerSync.cs` | マウント |
| `Services/MovementCommands.cs` | vnav / rotation コマンド |
| `Services/FrontlineAutomation.cs` | オーケストレーション |
| `Services/FrontlineLeaveAutomation.cs` | 結果画面退出 + SelectYesno 確認 |
| `Services/LeaveDialogText.cs` | 退出 Yesno 文言判定 |
| `Services/FrontlineDutyConfirmAutomation.cs` | コンテンツファインダー参加確定 |
| `Services/RotationSolverState.cs` | RSR 稼働状態の参照 |
| `Services/RotationModeAutomation.cs` | Off / Manual 時に Auto Big へ揃える |
| `UI/ConfigWindow.cs` | タブ付き設定ウィンドウ |
| `UI/GeneralTab.cs` | General タブ |
| `UI/DebugTab.cs` | Debug タブ |
| `UI/AflImGui.cs` | 共通 ImGui 部品 |
| `UI/DebugTable.cs` | Debug 用 KV テーブル |
| `UI/ConfigFooter.cs` | バージョン・サポートリンク |

## 定数（`FrontlineConstants`）

| 定数 | 値 |
|------|-----|
| 密集半径 | 50m |
| 近傍戦闘判定半径 | 30m |
| 近傍戦闘降下人数 | 3 人 |
| 移動オフセット | 1m ≤ r < 3m |
| 位置変化閾値 | 0.1m |

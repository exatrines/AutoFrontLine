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
| 1273 | 外縁遺跡群（制圧戦） |
| 431 | シールロック（争奪戦） |
| 554 | フィールド・オブ・グローリー（砕氷戦） |
| 888 | オンサル・ハカイル（終節戦） |
| 1313 | ウォーコー・チーテ（演習戦） |

## 設定 UI

| 項目 | 既定値 | 説明 |
|------|--------|------|
| `Enabled` | false | 追従・同期の有効化 |
| `MountSelectionId` | 0 | マウント（0 = ルーレット、所持分のみ選択可） |
| `MountDistanceMeters` | 30 | 乗馬を試行する移動先までの距離（m、0〜100） |
| `DismountEnemyDistanceMeters` | 20 | 敵がこの半径内なら降馬（m、0〜100） |
| `GroupMovementRefreshIntervalSeconds` | 1 | 集団移動モードの再判定・moveto 間隔（秒、0.5〜3.0） |
| `HostileModeRefreshIntervalSeconds` | 1 | 敵対モードの再判定・moveto 間隔（秒、0.5〜3.0） |
| `HostileModePositionRatio` | 0.5 | 敵対モードの立ち位置（0=先頭味方、1=最遠味方） |
| `FollowIntervalSeconds` | 1 | 移動コマンド間隔（秒、UI は整数 1〜60） |
| `PlayerReselectIntervalSeconds` | 3 | 追跡対象の再選択間隔（秒、UI は整数 1〜120） |

- コマンド: `/autofrontline`
  - 引数なし: 設定を開く
  - `on` / `off` / `toggle`: `Enabled` を変更（必須プラグイン未充足時は `on` を拒否しチャットに通知）
- タブ: **General**（必須プラグイン・推奨ジョブ）→ **Settings**（Enable・マウント・間隔）→ **Debug**（状態表示、タブ文字色グレー）
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
  └── ClosestEnemyPlayerTargeting.Update()
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

- **選定（通常）:**
  - **基本:** 生存メンバー（自分・前回追跡対象を除く）のうち、半径 **50m** 内の味方数が最大のプレイヤー（同数タイはランダム）。除外後に候補がいなければ前回対象の除外をやめて再選定
  - **追加（敵対モード）:** 自分から半径 **30m** 以内に敵がいるとき、最も近い敵を基準に **その敵から 30m 以内の味方（自分除く）** のみを対象とする。対象のうち敵に最も近い味方を追跡し、敵に最も近い **10 名** の先頭と最遠の間（`HostileModePositionRatio`、0=先頭・1=最遠、既定 **0.5**）へ移動。該当なしなら基本にフォールバック
- **再選択:** 未設定時、設定間隔経過時、追跡対象死亡時（死亡は即時）

## 移動

- **移動先:** 追跡対象の位置から水平 **1m 以上 3m 未満** のランダム点
- **スキップ:** 追跡対象が前回 moveto 時から **0.1m 未満** しか動いていなければ moveto しない
- **コマンド**（移動先があるフォロー周期のみ）: `/vnav moveto <X> <Y> <Z>`

## 戦闘ターゲット

`IsAutomationActive` かつフロントライン内。

- 対象: `IPlayerCharacter` のうち **自分・PT・アライアンス（収集メンバー）以外**、HP &gt; 0
- 選定: 自キャラから **最も近い** 敵プレイヤー
- 操作: `ITargetManager.Target` を設定（0.5s スロットル、既に同対象ならスキップ）

## Rotation Solver

`IsAutomationActive` かつフロントライン内・Rotation Solver ロード時。`RotationSolverState` が RSR の `DataCenter` を参照する。

| RSR 状態 | 動作 |
|---------|------|
| Manual | 何もしない |
| Manual 以外（Auto Off / Auto Big 等） | `/rotation manual`（2 秒スロットルで再送） |

## マウント

移動先（`LastMoveTarget`、未設定時は追跡対象の位置）までの距離が **設定の乗馬距離（m）以上** のときマウントを試行する（戦闘中でも可）。デフォルト 30、設定 0〜100。

設定のマウント選択肢は **所持しているマウントのみ**（`PlayerState.IsMountUnlocked`）。先頭はマウントルーレット。

| 状態 | 挙動 |
|------|------|
| 移動先まで乗馬距離以上・未マウント | 設定マウント（1.5s スロットル） |
| 移動先まで乗馬距離未満 | マウント試行しない（徒歩で moveto） |
| 設定の降馬距離（m）以内に敵プレイヤー（自分・PT・アライアンス以外）がいる | `/mount` で降下（1.5s スロットル）。デフォルト 20 |

- 移動先が乗馬距離以上かつ未マウントのとき `ShouldDeferMovement` が true（moveto を待つ）

| 設定項目 | キー | 範囲 | デフォルト |
|---------|------|------|------------|
| 乗馬距離 | `MountDistanceMeters` | 0〜100 | 30 |
| 降馬距離（敵） | `DismountEnemyDistanceMeters` | 0〜100 | 20 |

## ダイアログ自動操作

`IsAutomationActive` 時のみ。

| アドオン | 動作 |
|---------|------|
| `ContentsFinderConfirm` | タイトルがデイリーフロントラインのときのみ `Commence()` |
| `FrontlineRecord` / `FrontLineRecord` | `PostSetup` 時のみ `Callback.Fire(-1)` で退出要求（YesAlready と同じ） |
| `SelectYesno` | 上記退出要求後の `PostSetup` かつ退出確認文言一致時のみ `Yes` |

フレームごとの Record ポーリングは行わない（入場直後の誤退出防止）。

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
| `Services/ClosestEnemyPlayerTargeting.cs` | 最近接敵プレイヤーをターゲット |
| `Services/CombatAllyFilters.cs` | 味方 ContentId 集合 |
| `Services/RotationModeAutomation.cs` | Manual 以外を Manual へ揃える |
| `UI/ConfigWindow.cs` | タブ付き設定ウィンドウ |
| `UI/GeneralTab.cs` | General タブ |
| `UI/SettingsTab.cs` | Settings タブ |
| `UI/DebugTab.cs` | Debug タブ |
| `UI/AflImGui.cs` | 共通 ImGui 部品 |
| `UI/DebugTable.cs` | Debug 用 KV テーブル |
| `UI/ConfigFooter.cs` | バージョン・サポートリンク |

## 定数（`FrontlineConstants`）

| 定数 | 値 |
|------|-----|
| 密集半径 | 50m |
| 敵接近追従半径 | 30m |
| 乗馬距離 / 降馬距離（敵） | 設定（0〜100m、既定 30 / 20） |
| 移動オフセット | 1m ≤ r < 3m |
| 位置変化閾値 | 0.1m |

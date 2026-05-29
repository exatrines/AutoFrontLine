# Auto Frontline

FF14 Dalamud plugin that automates Frontline movement and combat.

## Required plugins (not bundled)

- [vnavmesh](https://github.com/awgil/ffxiv_navmesh) — `/vnav moveto <X> <Y> <Z>`
- [Rotation Solver Reborn](https://github.com/FFXIV-CombatReborn/RotationSolverReborn) — `/rotation Off` / `/rotation Auto`

## Install (custom repo)

`pluginmaster.json` はこのリポジトリ外（別リポジトリ）で管理しています。

Dalamud → Settings → Experimental → **Custom Plugin Repositories** に、その `pluginmaster.json` の raw URL を登録し、**Auto Frontline** をインストールしてください。

## Build

```powershell
git submodule update --init --recursive
dotnet build AutoFrontline.sln -c Release
```

## In-game

- `/autofrontline` — open settings (no arguments)
- `/autofrontline on|off|toggle` — enable or disable without opening settings
- **General**: required plugins, Enable, intervals
- **Debug**: field, tracked player, movement state

## Release (maintainers)

1. Bump version (four-part `AssemblyVersion`, tag `v` + version):

   ```bash
   bash .github/scripts/bump-version.sh 1.0.0.0
   ```

2. Commit, tag, and push（`main` だけでは Release は走りません。タグ必須）:

   ```bash
   git add AutoFrontline/AutoFrontline.json AutoFrontline/AutoFrontline.csproj CHANGELOG.md
   git commit -m "Release 1.0.0.0"
   git tag v1.0.0.0
   git push origin main --follow-tags
   ```

3. 外部リポジトリの **`pluginmaster.json` も同じバージョンに更新**（`AssemblyVersion`・`DownloadLinkInstall` / `DownloadLinkUpdate`）。

4. GitHub Actions **Release** workflow builds `AutoFrontline.zip` and publishes it to [Releases](https://github.com/exatrines/AutoFrontline/releases).

   `v*` タグの push で自動実行。Actions タブから手動実行する場合は tag に `v1.0.0.0` を指定。

`AutoFrontline/AutoFrontline.json` の `AssemblyVersion` とタグ（例: `v1.0.0.0`）は一致している必要があります。

## License

AGPL-3.0-or-later

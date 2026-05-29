# Auto Frontline

<div align="center">
<img src="./AutoFrontline/images/icon.png" width="300px">
</div>

## About

FF14 Dalamud plugin that automates Frontline movement and combat.

## Required Plugins

- [vnavmesh](https://github.com/awgil/ffxiv_navmesh)
- [Rotation Solver Reborn](https://github.com/FFXIV-CombatReborn/RotationSolverReborn)

## Plugin Repository URL

```
https://raw.githubusercontent.com/exatrines/DalamudPlugins/refs/heads/main/pluginmaster.json
```

## In-game

- `/autofrontline` — open settings
- `/autofrontline on|off` — enable or disable plugin
- `/autofrontline toggle` — toggle plugin
- **General**: required plugins, Enable, intervals
- **Debug**: field, tracked player, movement state

## Build

```bash
git submodule update --init --recursive
dotnet build AutoFrontline.sln -c Release
```

## Release (maintainers)

```bash
bash .github/scripts/bump-version.sh 1.0.0.0
git add AutoFrontline/AutoFrontline.json AutoFrontline/AutoFrontline.csproj CHANGELOG.md
git commit -m "Release 1.0.0.0"
git tag v1.0.0.0
git push origin main
git push origin  v1.0.0.0
```

## License

AGPL-3.0-or-later

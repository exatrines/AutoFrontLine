using System.Collections.Generic;
using System.Linq;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;

namespace AutoFrontline.Services;

/// <summary>設定 UI 用マウント一覧（先頭はマウントルーレット、以降は所持分のみ）。</summary>
internal readonly record struct MountOption(uint SelectionId, string DisplayName);

internal static unsafe class MountCatalog
{
    /// <summary>マウントルーレット（GeneralAction）。</summary>
    public const uint RouletteSelectionId = 0;

    private static MountOption[] cachedOptions = [];
    private static ulong cachedForContentId;
    private static uint? cachedRouletteActionId;

    public static void InvalidateCache()
    {
        cachedOptions = [];
        cachedForContentId = 0;
        cachedRouletteActionId = null;
    }

    public static IReadOnlyList<MountOption> GetOptions()
    {
        var contentId = Player.CID;
        if (cachedOptions.Length > 0 && cachedForContentId == contentId)
            return cachedOptions;

        cachedForContentId = contentId;
        cachedOptions = BuildOptions();
        return cachedOptions;
    }

    public static bool IsMountOwned(uint mountRowId)
    {
        if (mountRowId == 0 || mountRowId == RouletteSelectionId)
            return false;

        var state = PlayerState.Instance();
        return state != null && state->IsMountUnlocked(mountRowId);
    }

    public static string GetDisplayName(uint selectionId)
    {
        foreach (var option in GetOptions())
        {
            if (option.SelectionId == selectionId)
                return option.DisplayName;
        }

        if (selectionId == RouletteSelectionId)
            return GetRouletteDisplayName();

        if (Svc.Data.GetExcelSheet<Mount>().GetRowOrDefault(selectionId) is { } mount)
        {
            var name = mount.Singular.ToString();
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        return $"Mount #{selectionId}";
    }

    public static uint ResolveRouletteActionId() =>
        cachedRouletteActionId ??= FindRouletteActionId() ?? FrontlineConstants.MountRouletteGeneralActionId;

    private static MountOption[] BuildOptions()
    {
        var state = PlayerState.Instance();
        if (state == null || state->NumOwnedMounts == 0)
            return [];

        var options = new List<MountOption> { new(RouletteSelectionId, GetRouletteDisplayName()) };

        foreach (var mount in Svc.Data.GetExcelSheet<Mount>()
                     .Where(m => m.RowId != 0 && state->IsMountUnlocked(m.RowId))
                     .OrderBy(m => m.Singular.ToString(), StringComparer.CurrentCulture))
        {
            var name = mount.Singular.ToString();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            options.Add(new MountOption(mount.RowId, name));
        }

        return options.ToArray();
    }

    private static string GetRouletteDisplayName()
    {
        var actionId = ResolveRouletteActionId();
        return Svc.Data.GetExcelSheet<GeneralAction>().GetRowOrDefault(actionId)?.Name.ToString()
               ?? "Mount Roulette";
    }

    private static uint? FindRouletteActionId()
    {
        foreach (var row in Svc.Data.GetExcelSheet<GeneralAction>())
        {
            var name = row.Name.ToString();
            if (name is "Mount Roulette" or "マウントルーレット")
                return row.RowId;
        }

        return null;
    }
}

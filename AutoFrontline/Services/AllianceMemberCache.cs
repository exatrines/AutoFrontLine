using System.Collections.Generic;
using ECommons.DalamudServices;

namespace AutoFrontline.Services;

/// <summary>フレーム内のアライアンスメンバー収集結果を共有する。</summary>
internal static class AllianceMemberCache
{
    private static uint cachedTerritoryId;
    private static int frameGeneration = -1;
    private static int cachedGeneration = -1;
    private static List<AllianceMemberSnapshot> cachedMembers = [];

    public static void BeginFrame() => frameGeneration++;

    public static List<AllianceMemberSnapshot> GetMembers()
    {
        var territoryId = Svc.ClientState.TerritoryType;
        if (cachedGeneration == frameGeneration && cachedTerritoryId == territoryId)
            return cachedMembers;

        cachedTerritoryId = territoryId;
        cachedGeneration = frameGeneration;
        cachedMembers = AllianceMemberCollector.Collect();
        return cachedMembers;
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace AutoFrontline.Services;

/// <summary>半径内の敵対プレイヤー（自分・PT・アライアンス以外）を検出する。</summary>
internal static unsafe class NearbyEnemyDetector
{
    public static bool HasNearbyEnemy(IReadOnlyList<AllianceMemberSnapshot> allies, out int countWithinRadius)
    {
        countWithinRadius = 0;
        foreach (var _ in EnumerateEnemiesWithinRadius(allies, C.DismountEnemyDistanceMeters))
            countWithinRadius++;

        return countWithinRadius > 0;
    }

    public static bool TryGetClosestEnemy(
        IReadOnlyList<AllianceMemberSnapshot> allies,
        float radiusMeters,
        out IPlayerCharacter closest) =>
        TryGetClosestEnemy(allies, radiusMeters, out closest, out _);

    public static bool TryGetClosestEnemy(
        IReadOnlyList<AllianceMemberSnapshot> allies,
        out IPlayerCharacter closest,
        out float distanceMeters) =>
        TryGetClosestEnemy(allies, float.MaxValue, out closest, out distanceMeters);

    public static bool TryGetClosestEnemy(
        IReadOnlyList<AllianceMemberSnapshot> allies,
        float radiusMeters,
        out IPlayerCharacter closest,
        out float distanceMeters)
    {
        closest = null!;
        distanceMeters = float.MaxValue;

        if (!Player.Available || Player.Object == null)
            return false;

        var selfPosition = Player.Object.Position;

        foreach (var enemy in EnumerateEnemiesWithinRadius(allies, radiusMeters))
        {
            var distance = Vector3.Distance(selfPosition, enemy.Position);
            if (distance >= distanceMeters)
                continue;

            distanceMeters = distance;
            closest = enemy;
        }

        return closest != null;
    }

    private static IEnumerable<IPlayerCharacter> EnumerateEnemiesWithinRadius(
        IReadOnlyList<AllianceMemberSnapshot> allies,
        float radiusMeters)
    {
        if (!Player.Available || Player.Object == null)
            yield break;

        var allyContentIds = CombatAllyFilters.BuildContentIds(allies);
        var selfPosition = Player.Object.Position;
        var radiusSq = radiusMeters >= float.MaxValue / 2f
            ? float.MaxValue
            : radiusMeters * radiusMeters;

        foreach (var pc in Svc.Objects.OfType<IPlayerCharacter>())
        {
            if (pc.GameObjectId == Player.Object.GameObjectId)
                continue;

            var contentId = GetContentId(pc);
            if (contentId == 0 || allyContentIds.Contains(contentId))
                continue;

            if (pc.CurrentHp == 0)
                continue;

            if (Vector3.DistanceSquared(selfPosition, pc.Position) > radiusSq)
                continue;

            yield return pc;
        }
    }

    private static unsafe ulong GetContentId(IPlayerCharacter pc) => pc.Struct()->ContentId;
}

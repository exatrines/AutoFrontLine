using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace AutoFrontline.Services;

/// <summary>フロントライン中のアライアンス／パーティ／CW メンバー収集。</summary>
public static unsafe class AllianceMemberCollector
{
    public static string LastCollectionSource { get; private set; } = string.Empty;

    public static List<AllianceMemberSnapshot> Collect()
    {
        if (!FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
        {
            LastCollectionSource = string.Empty;
            return [];
        }

        var result = new List<AllianceMemberSnapshot>();
        var seen = new HashSet<ulong>();
        var sources = new List<string>();

        var count = result.Count;
        CollectAlliance(result, seen);
        if (result.Count > count)
            sources.Add("alliance");

        count = result.Count;
        CollectParty(result, seen);
        if (result.Count > count)
            sources.Add("party");

        count = result.Count;
        CollectCrossRealm(result, seen);
        if (result.Count > count)
            sources.Add("crossrealm");

        LastCollectionSource = sources.Count > 0 ? string.Join(" + ", sources) : "none";
        return result;
    }

    private static void CollectParty(List<AllianceMemberSnapshot> result, HashSet<ulong> seen)
    {
        var group = GroupManager.Instance()->GetGroup();
        if (group == null)
            return;

        for (var i = 0; i < 8; i++)
            TryAddPartyMember(result, seen, group->GetPartyMemberByIndex(i));

        if (!Player.Available || Player.Object == null || Player.CID == 0 || !seen.Add(Player.CID))
            return;

        result.Add(ToSnapshot(
            Player.Name,
            Player.CID,
            Player.Object.EntityId,
            Player.Object.Position,
            Player.IsDead));
    }

    private static void CollectAlliance(List<AllianceMemberSnapshot> result, HashSet<ulong> seen)
    {
        var group = GroupManager.Instance()->GetGroup();
        if (group == null)
            return;

        for (var partyIndex = 0; partyIndex < 3; partyIndex++)
        {
            for (var memberIndex = 0; memberIndex < 8; memberIndex++)
                TryAddPartyMember(result, seen, group->GetAllianceMemberByGroupAndIndex(partyIndex, memberIndex));
        }

        if (result.Count > 0)
            return;

        if (!group->IsAlliance && group->MemberCount <= 8)
            return;

        for (var i = 0; i < group->MemberCount; i++)
            TryAddPartyMember(result, seen, group->GetAllianceMemberByIndex(i));
    }

    private static void CollectCrossRealm(List<AllianceMemberSnapshot> result, HashSet<ulong> seen)
    {
        var proxy = InfoProxyCrossRealm.Instance();
        if (proxy == null)
            return;

        for (var gi = 0; gi < proxy->GroupCount; gi++)
        {
            var crossGroup = proxy->CrossRealmGroups[gi];
            for (var mi = 0; mi < crossGroup.GroupMemberCount; mi++)
            {
                var m = crossGroup.GroupMembers[mi];
                if (m.ContentId == 0 || !seen.Add(m.ContentId))
                    continue;

                var name = GenericHelpers.Read(m.Name);
                var resolved = TryResolveLiveMember(m.EntityId, name, (uint)m.HomeWorld);
                if (resolved == null)
                    continue;

                result.Add(ToSnapshot(name, m.ContentId, m.EntityId, resolved.Value.Position, resolved.Value.IsDead));
            }
        }
    }

    private static void TryAddPartyMember(List<AllianceMemberSnapshot> result, HashSet<ulong> seen, PartyMember* pm)
    {
        if (pm == null || pm->ContentId == 0 || !seen.Add(pm->ContentId))
            return;

        if ((pm->Flags & 0x01) == 0 && pm->EntityId == 0)
            return;

        var name = GenericHelpers.Read(pm->Name);
        var isDead = pm->CurrentHP == 0;
        var position = pm->Position;

        if (pm->EntityId != 0)
        {
            var obj = Svc.Objects.SearchByEntityId(pm->EntityId);
            if (obj != null)
            {
                position = obj.Position;
                if (obj is ICharacter ch)
                    isDead = ch.CurrentHp == 0;
            }
        }
        else if ((pm->Flags & 0x04) == 0)
        {
            return;
        }

        result.Add(ToSnapshot(name, pm->ContentId, pm->EntityId, position, isDead));
    }

    private static (Vector3 Position, bool IsDead)? TryResolveLiveMember(uint entityId, string name, uint homeWorld)
    {
        if (entityId != 0)
        {
            var byEntity = Svc.Objects.SearchByEntityId(entityId);
            if (byEntity != null)
            {
                var dead = byEntity is ICharacter ch && ch.CurrentHp == 0;
                return (byEntity.Position, dead);
            }
        }

        var pc = Svc.Objects.FirstOrDefault(x =>
            x is IPlayerCharacter p
            && p.HomeWorld.RowId == homeWorld
            && x.Name.ToString() == name);

        if (pc == null)
            return null;

        var isDead = pc is ICharacter character && character.CurrentHp == 0;
        return (pc.Position, isDead);
    }

    private static AllianceMemberSnapshot ToSnapshot(
        string name,
        ulong contentId,
        uint entityId,
        Vector3 position,
        bool isDead) =>
        new()
        {
            Name = name,
            ContentId = contentId,
            EntityId = entityId,
            Position = position,
            IsDead = isDead,
        };
}

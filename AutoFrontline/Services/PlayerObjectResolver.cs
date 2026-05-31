using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace AutoFrontline.Services;

internal static unsafe class PlayerObjectResolver
{
    public static IPlayerCharacter FindByContentId(ulong contentId)
    {
        if (contentId == 0)
            return null;

        foreach (var pc in Svc.Objects.OfType<IPlayerCharacter>())
        {
            if (pc.Struct()->ContentId == contentId)
                return pc;
        }

        return null;
    }

    public static IPlayerCharacter FindByNameAndWorld(string name, uint homeWorldRowId) =>
        Svc.Objects.OfType<IPlayerCharacter>()
            .FirstOrDefault(pc =>
                pc.Name.ToString() == name
                && pc.HomeWorld.RowId == homeWorldRowId);

    public static IGameObject FindByEntityId(uint entityId) =>
        entityId == 0 ? null : Svc.Objects.SearchByEntityId(entityId);

    public static IGameObject ResolveFromSnapshot(AllianceMemberSnapshot snapshot)
    {
        if (snapshot.EntityId != 0 && FindByEntityId(snapshot.EntityId) is { } byEntity)
            return byEntity;

        return Svc.Objects.OfType<IPlayerCharacter>()
            .FirstOrDefault(pc => pc.Name.ToString() == snapshot.Name);
    }
}

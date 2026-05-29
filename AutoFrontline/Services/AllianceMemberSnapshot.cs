using System.Numerics;

namespace AutoFrontline.Services;

public readonly struct AllianceMemberSnapshot
{
    public string Name { get; init; }
    public ulong ContentId { get; init; }
    public uint EntityId { get; init; }
    public Vector3 Position { get; init; }
    public bool IsDead { get; init; }
}

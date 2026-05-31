using System.Numerics;

namespace AutoFrontline.Services;

internal static class FollowMovePlanner
{
    public static Vector3 CreateDestination(Vector3 anchor, FollowSelectionMode mode)
    {
        if (mode == FollowSelectionMode.Hostile)
            return anchor;

        return RandomOffset(
            anchor,
            FrontlineConstants.MoveOffsetMinMeters,
            FrontlineConstants.MoveOffsetMaxMeters);
    }

    public static Vector3 RandomOffset(Vector3 anchor, float minMeters, float maxMeters)
    {
        var angle = Random.Shared.NextSingle() * MathF.PI * 2f;
        var dist = minMeters + Random.Shared.NextSingle() * (maxMeters - minMeters);
        return anchor + new Vector3(MathF.Cos(angle) * dist, 0f, MathF.Sin(angle) * dist);
    }
}

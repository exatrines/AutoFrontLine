namespace AutoFrontline.Services;

internal enum FollowSelectionMode
{
    None,
    GroupMovement,
    Hostile,
}

internal static class FollowSelectionModeExtensions
{
    public static string ToDebugLabel(this FollowSelectionMode mode) => mode switch
    {
        FollowSelectionMode.GroupMovement => "Densest",
        FollowSelectionMode.Hostile => "EnemyProximate",
        _ => string.Empty,
    };
}

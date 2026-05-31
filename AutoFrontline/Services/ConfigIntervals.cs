namespace AutoFrontline.Services;

internal static class ConfigIntervals
{
    public static int GroupMovementRefreshMs =>
        ToMilliseconds(C.GroupMovementRefreshIntervalSeconds, FrontlineConstants.ModeRefreshMinMs);

    public static int HostileModeRefreshMs =>
        ToMilliseconds(C.HostileModeRefreshIntervalSeconds, FrontlineConstants.ModeRefreshMinMs);

    private static int ToMilliseconds(float seconds, int minMs) =>
        Math.Max(minMs, (int)Math.Round(seconds * 1000f));
}

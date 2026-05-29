namespace AutoFrontline.Services;

internal static class ConfigIntervals
{
    public static int FollowMs => ToMilliseconds(C.FollowIntervalSeconds);

    public static int PlayerReselectMs => ToMilliseconds(C.PlayerReselectIntervalSeconds);

    private static int ToMilliseconds(float seconds) =>
        Math.Max(FrontlineConstants.ConfigIntervalMinMs, (int)seconds * 1000);
}

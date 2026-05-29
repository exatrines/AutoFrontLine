namespace AutoFrontline;

public sealed class Configuration
{
    public bool Enabled;
    public float FollowIntervalSeconds = 1f;
    public float PlayerReselectIntervalSeconds = 3f;

    // Legacy config key
    public float RecalculateIntervalSeconds
    {
        get => FollowIntervalSeconds;
        set => FollowIntervalSeconds = value;
    }
}

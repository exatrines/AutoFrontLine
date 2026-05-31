namespace AutoFrontline;

public sealed class Configuration
{
    public bool Enabled;
    /// <summary>0 = マウントルーレット。それ以外は <see cref="Mount"/> の RowId。</summary>
    public uint MountSelectionId;

    /// <summary>集団移動モード: 密集中心プレイヤー判定・moveto 間隔（秒）。</summary>
    public float GroupMovementRefreshIntervalSeconds = 1f;

    /// <summary>敵対モード: 最寄り敵・最寄り味方・中点判定・moveto 間隔（秒）。</summary>
    public float HostileModeRefreshIntervalSeconds = 1f;

    /// <summary>敵対モード: 先頭（敵に最も近い）〜最遠味方間の立ち位置（0=先頭、1=最遠、既定0.5=中央）。</summary>
    public float HostileModePositionRatio = 0.5f;

    /// <summary>移動先までこの距離以上ならマウントを試行（m）。</summary>
    public int MountDistanceMeters = 30;

    /// <summary>この半径内に敵プレイヤーがいればマウントを降ろす（m）。</summary>
    public int DismountEnemyDistanceMeters = 20;

    // Legacy config keys
    public float UpdateIntervalSeconds
    {
        get => GroupMovementRefreshIntervalSeconds;
        set => GroupMovementRefreshIntervalSeconds = value;
    }

    public float FollowIntervalSeconds
    {
        get => GroupMovementRefreshIntervalSeconds;
        set => GroupMovementRefreshIntervalSeconds = value;
    }

    public float PlayerReselectIntervalSeconds
    {
        get => GroupMovementRefreshIntervalSeconds;
        set => GroupMovementRefreshIntervalSeconds = value;
    }

    public float RecalculateIntervalSeconds
    {
        get => GroupMovementRefreshIntervalSeconds;
        set => GroupMovementRefreshIntervalSeconds = value;
    }

    public float NaviRebuildIntervalSeconds
    {
        get => GroupMovementRefreshIntervalSeconds;
        set => GroupMovementRefreshIntervalSeconds = value;
    }
}

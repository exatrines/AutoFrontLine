namespace AutoFrontline;

/// <summary>フロントライン自動化で使う固定値とスロットルキー。</summary>
internal static class FrontlineConstants
{
    public const float DensityRadiusMeters = 50f;
    public const float EnemyProximityFollowRadiusMeters = 30f;
    public const int EnemyProximityFrontlineAllyCount = 10;
    public const float MoveOffsetMinMeters = 1f;
    public const float MoveOffsetMaxMeters = 3f;
    public const float PositionUnchangedThresholdMeters = 0.1f;

    public const int MountThrottleMs = 1500;
    public const int DismountThrottleMs = 1500;

    public const uint MountRouletteGeneralActionId = 9;
    public const int LeaveYesnoThrottleMs = 300;
    public const int LeaveConfirmTimeoutMs = 60_000;
    public const int ContentsFinderConfirmThrottleMs = 500;
    public const int RotationManualIntervalMs = 2000;

    public const int ConfigIntervalMinMs = 100;
    public const int ModeRefreshMinMs = 500;

    public const string ThrottleMove = "AflMove";
    public const string ThrottleLeaveYesno = "AflLeaveYesno";
    public const string ThrottleContentsFinderConfirm = "AflContentsFinderConfirm";
    public const string ThrottleRotationManual = "AflRotationManual";
    public const string ThrottleEnemyTarget = "AflEnemyTarget";
    public const string ThrottleMount = "AflMount";
    public const string ThrottleDismount = "AflDismount";
}

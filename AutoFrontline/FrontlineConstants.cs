namespace AutoFrontline;

/// <summary>フロントライン自動化で使う固定値とスロットルキー。</summary>
internal static class FrontlineConstants
{
    public const float DensityRadiusMeters = 50f;
    public const float MoveOffsetMinMeters = 1f;
    public const float MoveOffsetMaxMeters = 3f;
    public const float PositionUnchangedThresholdMeters = 0.1f;

    public const float NearbyCombatRadiusMeters = 10f;
    public const int NearbyCombatDismountCount = 3;
    public const int MountThrottleMs = 1500;
    public const int DismountThrottleMs = 1500;
    public const int SheatheThrottleMs = 500;

    public const uint MountRouletteGeneralActionId = 9;
    public const int LeaveRecordThrottleMs = 500;
    public const int LeaveYesnoThrottleMs = 300;
    public const int LeaveConfirmTimeoutMs = 60_000;
    public const int ContentsFinderConfirmThrottleMs = 500;
    public const int RotationAutoBigIntervalMs = 2000;

    public const int ConfigIntervalMinMs = 1000;

    public const string ThrottleMove = "AflMove";
    public const string ThrottleLeaveRecord = "AflLeaveRecord";
    public const string ThrottleLeaveYesno = "AflLeaveYesno";
    public const string ThrottleContentsFinderConfirm = "AflContentsFinderConfirm";
    public const string ThrottleRotationAutoBig = "AflRotationAutoBig";
    public const string ThrottleMount = "AflMount";
    public const string ThrottleDismount = "AflDismount";
    public const string ThrottleSheathe = "AflSheathe";
}

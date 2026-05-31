using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using ECommons.UIHelpers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoFrontline.Services;

/// <summary>
/// 試合終了画面: FrontlineRecord の PostSetup で退出ボタン相当の Callback(-1) を送り、
/// 続く SelectYesno で退出確認のみ Yes。結果画面が閉じても pending は維持する。
/// </summary>
public static unsafe class FrontlineLeaveAutomation
{
    private static readonly string[] RecordAddonNames = ["FrontlineRecord", "FrontLineRecord"];

    private static bool recordScreenOpen;
    private static bool leaveRequestedForRecord;
    private static bool pendingLeaveConfirm;
    private static long leaveRequestedTick;

    public static bool PendingLeaveConfirm => pendingLeaveConfirm;
    public static bool IsRecordScreenVisible => recordScreenOpen;

    public static void Init()
    {
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, RecordAddonNames, OnRecordPostSetup);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, RecordAddonNames, OnRecordPreFinalize);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectYesno", OnSelectYesnoLifecycle);
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "SelectYesno", OnSelectYesnoLifecycle);
    }

    public static void Dispose()
    {
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, RecordAddonNames, OnRecordPostSetup);
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PreFinalize, RecordAddonNames, OnRecordPreFinalize);
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "SelectYesno", OnSelectYesnoLifecycle);
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "SelectYesno", OnSelectYesnoLifecycle);
        ResetLeaveState();
    }

    /// <summary>退出確認の再試行と、結果画面消失後のタイムアウト処理。</summary>
    public static void Update()
    {
        if (!RequiredPlugins.IsAutomationActive)
        {
            if (pendingLeaveConfirm)
                ResetLeaveState();
            return;
        }

        if (pendingLeaveConfirm)
        {
            if (IsLeaveConfirmTimedOut())
            {
                ResetLeaveState();
                return;
            }

            TryConfirmLeaveYesno();
        }

        if (recordScreenOpen && (!TryGetRecordAddon(out var addon) || !addon->IsVisible || !GenericHelpers.IsAddonReady(addon)))
            recordScreenOpen = false;
    }

    private static void OnRecordPostSetup(AddonEvent type, AddonArgs args)
    {
        if (!RequiredPlugins.IsAutomationActive)
            return;

        var addon = (AtkUnitBase*)args.Addon.Address;
        if (!IsFrontlineRecordAddon(addon))
            return;

        if (!addon->IsVisible || !GenericHelpers.IsAddonReady(addon) || !GenericHelpers.IsScreenReady())
            return;

        recordScreenOpen = true;
        TryRequestLeave(addon);
    }

    private static void OnRecordPreFinalize(AddonEvent type, AddonArgs args)
    {
        // YesNo 表示のため結果画面は閉じるが、退出確認待ちは維持する。
        recordScreenOpen = false;
    }

    private static void OnSelectYesnoLifecycle(AddonEvent type, AddonArgs args) =>
        TryConfirmLeaveYesno((AtkUnitBase*)args.Addon.Address);

    private static void TryRequestLeave(AtkUnitBase* addon)
    {
        if (!IsFrontlineRecordAddon(addon) || leaveRequestedForRecord)
            return;

        // YesAlready と同じ: FrontlineRecord の「退出」ボタン相当 UI コールバック（直接コマンド退出ではない）
        ECommons.Automation.Callback.Fire(addon, true, -1);
        leaveRequestedForRecord = true;
        pendingLeaveConfirm = true;
        leaveRequestedTick = Environment.TickCount64;
    }

    private static void TryConfirmLeaveYesno(AtkUnitBase* specificAddon = null)
    {
        if (!leaveRequestedForRecord || !pendingLeaveConfirm)
            return;

        if (specificAddon != null)
        {
            if (!specificAddon->IsVisible || !GenericHelpers.IsAddonReady(specificAddon))
                return;

            TryConfirmLeaveYesno(new AddonMaster.SelectYesno((nint)specificAddon));
            return;
        }

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleLeaveYesno, FrontlineConstants.LeaveYesnoThrottleMs))
            return;

        foreach (var yesno in AddonFinder.YesNo)
            TryConfirmLeaveYesno(yesno);
    }

    private static void TryConfirmLeaveYesno(AddonMaster.SelectYesno yesno)
    {
        if (!yesno.IsVisible || !GenericHelpers.IsAddonReady(yesno.Base))
            return;

        if (!IsLeaveConfirmationDialog(yesno))
            return;

        if (!TryClickYes(yesno))
            return;

        if (!yesno.IsVisible)
            ResetLeaveState();
    }

    private static bool IsLeaveConfirmationDialog(AddonMaster.SelectYesno yesno) =>
        LeaveDialogText.IsLeaveConfirmation(yesno.TextLegacy)
        || LeaveDialogText.IsLeaveConfirmation(yesno.Text);

    private static bool IsLeaveConfirmTimedOut() =>
        leaveRequestedTick != 0
        && Environment.TickCount64 - leaveRequestedTick > FrontlineConstants.LeaveConfirmTimeoutMs;

    private static void ResetLeaveState()
    {
        recordScreenOpen = false;
        leaveRequestedForRecord = false;
        pendingLeaveConfirm = false;
        leaveRequestedTick = 0;
    }

    private static bool IsFrontlineRecordAddon(AtkUnitBase* addon)
    {
        if (addon == null)
            return false;

        var name = addon->NameString;
        if (string.IsNullOrEmpty(name))
            return false;

        foreach (var recordName in RecordAddonNames)
        {
            if (name.Equals(recordName, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static bool TryGetRecordAddon(out AtkUnitBase* addon)
    {
        foreach (var name in RecordAddonNames)
        {
            if (GenericHelpers.TryGetAddonByName(name, out addon) && IsFrontlineRecordAddon(addon))
                return true;
        }

        addon = null;
        return false;
    }

    private static bool TryClickYes(AddonMaster.SelectYesno yesno)
    {
        try
        {
            yesno.RespectDisabledButtons = false;
            yesno.Yes();
            ECommons.Automation.Callback.Fire(yesno.Base, true, 0);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

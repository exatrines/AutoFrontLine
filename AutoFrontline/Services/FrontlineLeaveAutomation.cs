using ECommons.UIHelpers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoFrontline.Services;

/// <summary>
/// 試合終了画面: FrontlineRecord で Callback.Fire(-1) し、続く SelectYesno を Yes。
/// YesAlready の FrontlineRecord と同じコールバック。YesNo は本プラグインで処理する。
/// </summary>
public static unsafe class FrontlineLeaveAutomation
{
    private static readonly string[] RecordAddonNames = ["FrontlineRecord", "FrontLineRecord"];

    private static bool leaveRequestedForRecord;
    private static bool pendingLeaveConfirm;
    private static long leaveRequestedTick;

    public static bool PendingLeaveConfirm => pendingLeaveConfirm;

    public static void Update()
    {
        if (!RequiredPlugins.IsAutomationActive)
            return;

        ExpirePendingConfirmIfTimedOut();

        if (TryConfirmYesno())
            return;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleLeaveRecord, FrontlineConstants.LeaveRecordThrottleMs))
            return;

        TryRequestLeave();
    }

    private static void ExpirePendingConfirmIfTimedOut()
    {
        if (!pendingLeaveConfirm)
            return;

        if (Environment.TickCount64 - leaveRequestedTick > FrontlineConstants.LeaveConfirmTimeoutMs)
            pendingLeaveConfirm = false;
    }

    private static bool TryConfirmYesno()
    {
        if (!pendingLeaveConfirm && !IsRecordAddonVisible())
            return false;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleLeaveYesno, FrontlineConstants.LeaveYesnoThrottleMs))
            return false;

        foreach (var yesno in AddonFinder.YesNo)
        {
            if (!yesno.IsVisible)
                continue;

            if (!pendingLeaveConfirm && !LeaveDialogText.IsLeaveConfirmation(yesno.TextLegacy))
                continue;

            if (!TryClickYes(yesno))
                continue;

            if (!yesno.IsVisible)
            {
                pendingLeaveConfirm = false;
                leaveRequestedForRecord = false;
            }

            return true;
        }

        return false;
    }

    private static void TryRequestLeave()
    {
        if (!TryGetRecordAddon(out var addon))
        {
            leaveRequestedForRecord = false;
            return;
        }

        if (!addon->IsVisible || !GenericHelpers.IsAddonReady(addon))
        {
            leaveRequestedForRecord = false;
            return;
        }

        if (leaveRequestedForRecord)
            return;

        ECommons.Automation.Callback.Fire(addon, true, -1);
        leaveRequestedForRecord = true;
        pendingLeaveConfirm = true;
        leaveRequestedTick = Environment.TickCount64;
    }

    private static bool IsRecordAddonVisible() =>
        TryGetRecordAddon(out var addon) && addon->IsVisible;

    private static bool TryGetRecordAddon(out AtkUnitBase* addon)
    {
        foreach (var name in RecordAddonNames)
        {
            if (GenericHelpers.TryGetAddonByName(name, out addon))
                return true;
        }

        addon = null;
        return false;
    }

    private static bool TryClickYes(AddonMaster.SelectYesno yesno)
    {
        var addon = (AddonSelectYesno*)yesno.Base;
        var yesButton = addon->YesButton;

        if (yesButton == null)
            return FireYesCallback(yesno.Base);

        EnableButtonIfDisabled(yesButton);

        if (yesButton->IsEnabled && yesButton->AtkResNode->IsVisible())
        {
            yesno.Yes();
            if (!yesno.IsVisible)
                return true;
        }

        if (FireYesCallback(yesno.Base))
            return !yesno.Base->IsVisible;

        yesno.Yes();
        return !yesno.IsVisible;
    }

    private static bool FireYesCallback(AtkUnitBase* addon)
    {
        try
        {
            ECommons.Automation.Callback.Fire(addon, true, 0);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void EnableButtonIfDisabled(AtkComponentButton* button)
    {
        if (button == null || button->IsEnabled)
            return;

        var flagsPtr = (ushort*)&button->AtkComponentBase.OwnerNode->AtkResNode.NodeFlags;
        *flagsPtr ^= 1 << 5;
    }
}

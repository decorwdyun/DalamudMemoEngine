using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using MemoEngine;
using MemoEngine.Models;

namespace DalamudMemoEngine;

internal sealed unsafe class StatusManager
{
    private Hook<StatusAppliedDelegate>? _statusAppliedHook;
    private Hook<StatusRemovedDelegate>? _statusRemovedHook;
    
    internal void Enable()
    {
        _statusAppliedHook ??=
            DalamudService.Hook.HookFromSignature<StatusAppliedDelegate>("48 8B C4 55 57 41 54 41 56", OnStatusApplied);
        _statusAppliedHook?.Enable();

        _statusRemovedHook ??=
            DalamudService.Hook.HookFromSignature<StatusRemovedDelegate>("E8 ?? ?? ?? ?? FF C6 48 83 C3 10 83 FE 3C",
                OnStatusRemoved);
        _statusRemovedHook?.Enable();
    }

    internal void Disable()
    {
        _statusAppliedHook?.Dispose();
        _statusAppliedHook = null;

        _statusRemovedHook?.Dispose();
        _statusRemovedHook = null;
    }

    private void OnStatusApplied(BattleChara** player, ushort statusId, float remainingTime, ushort statusParam,
        ulong sourceId, ushort stackCount)
    {
        _statusAppliedHook?.Original(player, statusId, remainingTime, statusParam, sourceId, stackCount);
        try
        {
            if (Context.Lifecycle == EngineState.Idle) return;

            Event.Status.RaiseApplied(DalamudService.TimeProvider.Now, (*player)->EntityId, statusId);
        }
        catch (Exception e)
        {
            DalamudService.Log.Error(e, "Failed to process status applied");
        }
    }

    private void OnStatusRemoved(BattleChara** player, ushort statusId, ushort statusParam, ulong sourceId,
        ushort stackCount)
    {
        _statusRemovedHook?.Original(player, statusId, statusParam, sourceId, stackCount);
        try
        {
            if (Context.Lifecycle == EngineState.Idle) return;

            Event.Status.RaiseRemoved(DalamudService.TimeProvider.Now, (*player)->EntityId, statusId);
        }
        catch (Exception e)
        {
            DalamudService.Log.Error(e, "Failed to process status removed");
        }
    }

    private delegate void StatusAppliedDelegate(BattleChara** player, ushort statusId, float remainingTime,
        ushort statusParam, ulong sourceId, ushort stackCount);

    private delegate void StatusRemovedDelegate(BattleChara** player, ushort statusId, ushort statusParam,
        ulong sourceId, ushort stackCount);
}
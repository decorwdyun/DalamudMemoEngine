using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using MemoEngine;
using MemoEngine.Models;

namespace DalamudMemoEngine;

internal sealed unsafe class ActionManager
{
    private Hook<ActionCompleteDelegate>? _actionCompletedHook;
    private Hook<ActionStartDelegate>? _actionStartHook;

    internal void Enable()
    {
        _actionStartHook ??=
            DalamudService.Hook.HookFromSignature<ActionStartDelegate>("E8 ?? ?? ?? ?? 80 7E 22 11", ActionStartDetour);
        _actionStartHook?.Enable();

        _actionCompletedHook ??=
            DalamudService.Hook.HookFromSignature<ActionCompleteDelegate>(
                "E8 ?? ?? ?? ?? 41 0F B6 56 ?? 44 0F 28 8C 24 ?? ?? ?? ??", ActionCompleteDetour);
        _actionCompletedHook?.Enable();
    }

    internal void Disable()
    {
        _actionStartHook?.Dispose();
        _actionStartHook = null;

        _actionCompletedHook?.Dispose();
        _actionCompletedHook = null;
    }

    private nint ActionStartDetour(BattleChara* player, ActionType type, uint actionId, nint a4, float rotation,
        float a6)
    {
        var original = _actionStartHook!.Original(player, type, actionId, a4, rotation, a6);
        if (player == null) return original;

        try
        {
            if (player->ObjectKind == ObjectKind.BattleNpc && Context.Lifecycle != EngineState.Idle)
                Event.Action.RaiseStarted(DalamudService.TimeProvider.Now, player->BaseId,
                    actionId);
        }
        catch (Exception e)
        {
            DalamudService.Log.Error(e, "Failed to raise action started event");
        }

        return original;
    }

    private nint ActionCompleteDetour
    (
        BattleChara* player,
        ActionType type,
        uint actionId,
        uint spellId,
        GameObjectId a5,
        nint a6,
        float rotation,
        short a8,
        int a9,
        int a10
    )
    {
        var original = _actionCompletedHook!.Original(player, type, actionId, spellId, a5, a6, rotation, a8, a9, a10);
        if (player == null) return original;

        try
        {
            if (player->ObjectKind == ObjectKind.BattleNpc && Context.Lifecycle != EngineState.Idle)
                Event.Action.RaiseCompleted(DalamudService.TimeProvider.Now, player->BaseId,
                    actionId);
        }
        catch (Exception e)
        {
            DalamudService.Log.Error(e, "Failed to raise action completed event");
        }

        return original;
    }

    private delegate nint ActionStartDelegate(BattleChara* player, ActionType type, uint actionId, nint a4,
        float rotation, float a6);

    private delegate nint ActionCompleteDelegate(
        BattleChara* player, ActionType type, uint actionId, uint spellId, GameObjectId a5, nint a6, float rotation,
        short a8, int a9, int a10);
}
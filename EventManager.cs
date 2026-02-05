using Dalamud.Game.ClientState.Conditions;
using MemoEngine.Models;

namespace DalamudMemoEngine;

internal sealed class EventManager
{
    private ActionManager? _actionService;
    private CombatantManager? _combatantService;
    private StatusManager? _statusService;

    private bool _enabled;

    internal void Enable()
    {
        if (_enabled) return;
        _enabled = true;

        // GENERAL EVENTS
        DalamudService.ClientState.TerritoryChanged += OnTerritoryChanged;

        // ACTION EVENTS
        _actionService = new ActionManager();
        _actionService.Enable();

        // COMBATANT EVENTS
        _combatantService = new CombatantManager();
        _combatantService.Enable();

        // STATUS EVENTS
        _statusService = new StatusManager();
        _statusService.Enable();

        // DUTY EVENTS
        DalamudService.DutyState.DutyCompleted += OnDutyCompleted;
        DalamudService.DutyState.DutyWiped += OnDutyWiped;

        // CONDITION EVENTS
        DalamudService.Condition.ConditionChange += OnConditionChange;
    }

    internal void Disable()
    {
        if (!_enabled) return;
        _enabled = false;

        // GENERAL EVENTS
        DalamudService.ClientState.TerritoryChanged -= OnTerritoryChanged;

        // ACTION EVENTS
        _actionService?.Disable();

        // COMBATANT EVENTS
        _combatantService?.Disable();

        // STATUS EVENTS
        _statusService?.Disable();

        // DUTY EVENTS
        DalamudService.DutyState.DutyCompleted -= OnDutyCompleted;
        DalamudService.DutyState.DutyWiped -= OnDutyWiped;

        // CONDITION EVENTS
        DalamudService.Condition.ConditionChange -= OnConditionChange;
    }

    private static void OnTerritoryChanged(ushort zoneId)
    {
        MemoEngine.Event.General.RaiseTerritoryChanged(DalamudService.TimeProvider.Now, zoneId);
    }

    private static void OnDutyCompleted(object? sender, ushort e)
    {
        MemoEngine.Event.General.RaiseDutyCompleted(DalamudService.TimeProvider.Now);
    }

    private static void OnDutyWiped(object? sender, ushort e)
    {
        MemoEngine.Event.General.RaiseDutyWiped(DalamudService.TimeProvider.Now);
    }

    private static void OnConditionChange(ConditionFlag flag, bool value)
    {
        if (flag != ConditionFlag.InCombat)
            return;

        if (value)
            MemoEngine.Event.General.RaiseCombatOptIn(DalamudService.TimeProvider.Now, GetPartySnapshots());
        else
            MemoEngine.Event.General.RaiseCombatOptOut(DalamudService.TimeProvider.Now);
    }

    private static Dictionary<uint, PlayerPayload> GetPartySnapshots()
    {
        if (DalamudService.PartyList.Length > 1)
            return DalamudService.PartyList.ToDictionary
            (
                p => p.EntityId,
                p => new PlayerPayload
                {
                    Name = p.Name.ToString(),
                    Server = p.World.Value.Name.ToString(),
                    JobId = p.ClassJob.RowId,
                    Level = p.Level,
                    DeathCount = 0
                }
            );

        if (DalamudService.ObjectTable.LocalPlayer is not null)
            return new Dictionary<uint, PlayerPayload>
            {
                {
                    DalamudService.PlayerState.EntityId,
                    new PlayerPayload
                    {
                        Name = DalamudService.PlayerState.CharacterName,
                        Server = DalamudService.PlayerState.HomeWorld.Value.Name.ToString(),
                        JobId = DalamudService.PlayerState.ClassJob.RowId,
                        Level = (uint)DalamudService.PlayerState.Level,
                        DeathCount = 0
                    }
                }
            };

        return [];
    }
}

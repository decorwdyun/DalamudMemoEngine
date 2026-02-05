using Dalamud.Plugin.Services;
using DalamudMemoEngine.Utils;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using MemoEngine;
using MemoEngine.Models;

namespace DalamudMemoEngine;

internal sealed class CombatantManager
{
    private readonly Dictionary<uint, bool> _lastAlive = [];

    internal void Enable()
    {
        DalamudService.Framework.Update += OnUpdate;
    }

    internal void Disable()
    {
        DalamudService.Framework.Update -= OnUpdate;
        _lastAlive.Clear();
    }

    private unsafe void OnUpdate(IFramework framework)
    {
        if (!Throttler.Throttle("CombatantManager-Update", 500)) return;
        if (Context.Lifecycle == EngineState.Idle) return;

        if (Context.EnemyDataId != 0)
            if (CharacterManager.Instance()->BattleCharas.ToArray().FirstOrDefault
                (x => x != null &&
                      x.Value != null &&
                      x.Value->BaseId == Context.EnemyDataId
                ) is { Value: not null } enemy)
                Event.Combatant.RaiseHpUpdated
                (
                    DalamudService.TimeProvider.Now,
                    enemy.Value->BaseId,
                    enemy.Value->Health,
                    enemy.Value->MaxHealth
                );

        var currentAlive = new Dictionary<uint, bool>();
        if (DalamudService.PartyList.Length > 1)
            currentAlive = DalamudService.PartyList.ToDictionary(p => p.EntityId,
                p => p is { GameObject: { IsDead: false }, CurrentHP: > 0 });

        foreach (var kv in currentAlive)
        {
            if (!_lastAlive.TryGetValue(kv.Key, out var wasAlive) || !wasAlive || kv.Value)
                continue;

            Event.General.RaisePlayerDied(DalamudService.TimeProvider.Now, kv.Key);
        }

        _lastAlive.Clear();
        foreach (var kv in currentAlive)
            _lastAlive[kv.Key] = kv.Value;
    }
}
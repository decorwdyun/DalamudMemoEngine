using Dalamud.Plugin;
using DalamudMemoEngine.Time;
using MemoEngine;
using MemoEngine.Models;

namespace DalamudMemoEngine;

public sealed class DalamudMemoEngine : IDisposable
{
    public event Action<FightRecordPayload>? OnFightFinalized;

    private readonly EventManager _eventManager;
    private readonly Action<FightRecordPayload> _onFightFinalizedForwarder;
    private bool _disposed;

    public DalamudMemoEngine(IDalamudPluginInterface pluginInterface, Func<DateTimeOffset>? nowProvider = null)
    {
        _eventManager = new EventManager();

        var timeProvider = nowProvider is null ? TimeProviders.System : TimeProviders.From(nowProvider);
        DalamudService.Init(pluginInterface, timeProvider);

        _onFightFinalizedForwarder = payload =>
        {
            if (_disposed) return;
            OnFightFinalized?.Invoke(payload);
        };
        Context.OnFightFinalized += _onFightFinalizedForwarder;
    }

    public void Enable()
    {
        _eventManager.Enable();
    }

    public void Disable()
    {
        _eventManager.Disable();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _eventManager.Disable();

        Context.OnFightFinalized -= _onFightFinalizedForwarder;
        OnFightFinalized = null;
    }
}

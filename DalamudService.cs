using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DalamudMemoEngine.Time;

namespace DalamudMemoEngine;

#nullable disable
internal class DalamudService
{
    [PluginService] internal static IClientState ClientState { get; private set; }
    [PluginService] internal static IDutyState DutyState { get; private set; }
    [PluginService] internal static ICondition Condition { get; private set; }
    [PluginService] internal static IPartyList PartyList { get; private set; }
    [PluginService] internal static IObjectTable ObjectTable { get; private set; }
    [PluginService] internal static IFramework Framework { get; private set; }
    [PluginService] internal static IGameInteropProvider Hook { get; private set; }
    [PluginService] internal static IPlayerState PlayerState { get; private set; }
    [PluginService] internal static IPluginLog Log { get; private set; }
    internal static ITimeProvider TimeProvider { get; private set; } = TimeProviders.System;

    internal static void Init(IDalamudPluginInterface pi, ITimeProvider timeProvider)
    {
        pi.Create<DalamudService>();
        TimeProvider = timeProvider;
    }
}
using System.Collections.Concurrent;
using System.Diagnostics;

namespace DalamudMemoEngine.Utils;

internal static class Throttler
{
    private static readonly ConcurrentDictionary<string, ThrottleState> States = new();

    internal static bool Throttle(string key, int intervalMs)
    {
        var now = Stopwatch.GetTimestamp();
        var intervalTicks = intervalMs * (Stopwatch.Frequency / 1000);
        var state = States.GetOrAdd(key, static _ => new ThrottleState());
        var last = Interlocked.Read(ref state.LastTick);

        if (now - last < intervalTicks) return false;

        var original = Interlocked.CompareExchange(ref state.LastTick, now, last);

        return original == last;
    }

    private class ThrottleState
    {
        internal long LastTick;
    }
}
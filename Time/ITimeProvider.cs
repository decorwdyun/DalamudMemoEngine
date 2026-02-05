namespace DalamudMemoEngine.Time;

public interface ITimeProvider
{
    DateTimeOffset Now { get; }
}

public static class TimeProviders
{
    public static ITimeProvider System { get; } = new DelegateTimeProvider(static () => DateTimeOffset.Now);

    public static ITimeProvider From(Func<DateTimeOffset> nowProvider)
    {
        return nowProvider is null
            ? throw new ArgumentNullException(nameof(nowProvider))
            : new DelegateTimeProvider(nowProvider);
    }

    private sealed class DelegateTimeProvider(Func<DateTimeOffset> nowProvider) : ITimeProvider
    {
        public DateTimeOffset Now => nowProvider();
    }
}
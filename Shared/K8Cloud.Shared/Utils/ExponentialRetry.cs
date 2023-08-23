namespace K8Cloud.Shared.Utils;

public static class ExponentialRetry
{
    public static TimeSpan GetDelay(TimeSpan delay, int retryCount)
    {
        var random = new Random();
        var delta = Math.Max(
            delay.TotalMilliseconds,
            delay.TotalMilliseconds * Math.Pow(2, retryCount - 1)
        );

        var lowInterval = (int)(delta * 0.8);
        var highInterval = (int)(delta * 1.2);

        return TimeSpan.FromMilliseconds(random.Next(lowInterval, highInterval));
    }
}

namespace SignalRHub.Test.Common;

/// <summary>
/// Isolated <see cref="IMeterFactory"/> for one test, so collectors only see the measurements of that test.
/// </summary>
internal sealed class TestMeterFactory : IDisposable
{
    private readonly ServiceProvider _provider = new ServiceCollection().AddMetrics().BuildServiceProvider();

    public IMeterFactory MeterFactory => _provider.GetRequiredService<IMeterFactory>();

    public HubMetrics CreateHubMetrics(IConnectionRegistry registry) => new(MeterFactory, registry);

    public MetricCollector<T> Collect<T>(string instrumentName)
        where T : struct => new(MeterFactory, HubMetrics.MeterName, instrumentName);

    public void Dispose() => _provider.Dispose();
}

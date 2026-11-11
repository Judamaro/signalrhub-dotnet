namespace SignalRHub.App.Metrics;

public sealed class MetricsOptions
{
    public const string SectionName = "Metrics";

    /// <summary>
    /// Port that serves the Prometheus scraping endpoint. It must be one of the ports the server listens on
    /// and must not be published, so only Prometheus (inside the private network) can read the metrics.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 9464;

    /// <summary>Path of the Prometheus scraping endpoint.</summary>
    [Required]
    [RegularExpression("^/.*", ErrorMessage = "The path must start with '/'.")]
    public string Path { get; set; } = "/metrics";
}

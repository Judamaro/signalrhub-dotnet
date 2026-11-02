namespace SignalRHub.App.Authentication;

public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKeys";

    [Required]
    public string HeaderName { get; set; } = "X-Api-Key";

    /// <summary>Keys accepted from the backends allowed to publish notifications.</summary>
    [MinLength(1)]
    public IList<string> Keys { get; } = [];
}

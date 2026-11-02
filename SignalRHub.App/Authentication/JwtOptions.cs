namespace SignalRHub.App.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>HMAC-SHA256 key shared with the identity provider that issues client tokens.</summary>
    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;
}

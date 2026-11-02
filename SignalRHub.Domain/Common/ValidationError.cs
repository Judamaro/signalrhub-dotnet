namespace SignalRHub.Domain.Common;

/// <summary>
/// Validation failure that carries the error messages grouped by property name.
/// </summary>
public sealed record ValidationError(IReadOnlyDictionary<string, string[]> Errors) : Error("General.Validation", "One or more validation errors occurred.", ErrorType.Validation);

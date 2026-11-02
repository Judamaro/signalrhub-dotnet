namespace SignalRHub.Application.Abstractions.Messaging;

/// <summary>Marker for a read-only operation that returns <typeparamref name="TResponse"/>.</summary>
#pragma warning disable CA1040 // Marker interface used for generic constraints.
public interface IQuery<TResponse>;
#pragma warning restore CA1040

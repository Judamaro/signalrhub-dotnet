namespace SignalRHub.Application.Abstractions.Messaging;

/// <summary>Marker for an operation that changes state and returns <typeparamref name="TResponse"/>.</summary>
#pragma warning disable CA1040 // Marker interface used for generic constraints.
public interface ICommand<TResponse>;
#pragma warning restore CA1040

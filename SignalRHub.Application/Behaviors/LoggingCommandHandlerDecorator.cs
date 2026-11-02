namespace SignalRHub.Application.Behaviors;

/// <summary>
/// Decorator that logs the start, outcome and duration of every command.
/// </summary>
internal sealed class LoggingCommandHandlerDecorator<TCommand, TResponse>(ICommandHandler<TCommand, TResponse> inner, ILogger<LoggingCommandHandlerDecorator<TCommand, TResponse>> logger) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        string commandName = typeof(TCommand).Name;
        long start = Stopwatch.GetTimestamp();
        CommandLog.Processing(logger, commandName);

        Result<TResponse> result = await inner.Handle(command, cancellationToken).ConfigureAwait(false);
        double elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;

        if (result.IsSuccess)
            CommandLog.Completed(logger, commandName, elapsed);
        else
            CommandLog.Failed(logger, commandName, result.Error.Code, result.Error.Description, elapsed);

        return result;
    }
}

internal static partial class CommandLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing command {Command}")]
    public static partial void Processing(ILogger logger, string command);

    [LoggerMessage(Level = LogLevel.Information, Message = "Command {Command} completed in {ElapsedMs:0.0} ms")]
    public static partial void Completed(ILogger logger, string command, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Command {Command} failed with {ErrorCode}: {ErrorDescription} ({ElapsedMs:0.0} ms)")]
    public static partial void Failed(ILogger logger, string command, string errorCode, string errorDescription, double elapsedMs);
}

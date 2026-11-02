namespace SignalRHub.Application.Behaviors;

/// <summary>
/// Decorator that runs every FluentValidation validator registered for the command
/// before delegating to the real handler.
/// </summary>
internal sealed class ValidationCommandHandlerDecorator<TCommand, TResponse>(ICommandHandler<TCommand, TResponse> inner, IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        ValidationContext<TCommand> context = new(command);
        ValidationResult[] results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false);

        Dictionary<string, string[]> errors = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Distinct(StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);

        if (errors.Count > 0)
            return Result.Failure<TResponse>(new ValidationError(errors));

        return await inner.Handle(command, cancellationToken).ConfigureAwait(false);
    }
}

namespace SignalRHub.Application.Notifications.Send;

internal sealed class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(c => c.Target).NotNull();

        RuleFor(c => c.Type).NotEmpty().MaximumLength(Notification.TypeMaxLength);
    }
}

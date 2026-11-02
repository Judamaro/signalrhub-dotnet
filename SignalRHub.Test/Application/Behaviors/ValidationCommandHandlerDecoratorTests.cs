namespace SignalRHub.Test.Application.Behaviors;

public sealed class ValidationCommandHandlerDecoratorTests
{
    private readonly ICommandHandler<SendNotificationCommand, SendNotificationResponse> _inner = Substitute.For<ICommandHandler<SendNotificationCommand, SendNotificationResponse>>();

    private readonly ValidationCommandHandlerDecorator<SendNotificationCommand, SendNotificationResponse> _decorator;

    public ValidationCommandHandlerDecoratorTests()
    {
        _decorator = new(_inner, [new SendNotificationCommandValidator()]);
    }

    [Fact]
    public async Task Short_circuits_with_validation_error_when_command_is_invalid()
    {
        SendNotificationCommand command = new(new ClientTarget(TestData.Client()), Type: "", Payload: null);

        Result<SendNotificationResponse> result = await _decorator.Handle(command, CancellationToken.None);

        ValidationError error = result.Error.ShouldBeOfType<ValidationError>();
        error.Errors.ShouldContainKey(nameof(SendNotificationCommand.Type));
        await _inner.DidNotReceiveWithAnyArgs().Handle(default!, default);
    }

    [Fact]
    public async Task Delegates_to_inner_handler_when_command_is_valid()
    {
        SendNotificationCommand command = new(new ClientTarget(TestData.Client()), "ping", null);
        SendNotificationResponse expected = new(Guid.NewGuid(), 1, TestData.Now);
        _inner.Handle(command, Arg.Any<CancellationToken>()).Returns(expected);

        Result<SendNotificationResponse> result = await _decorator.Handle(command, CancellationToken.None);

        result.Value.ShouldBe(expected);
    }
}

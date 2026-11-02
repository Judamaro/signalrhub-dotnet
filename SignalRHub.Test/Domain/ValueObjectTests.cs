namespace SignalRHub.Test.Domain;

public sealed class ValueObjectTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ClientId_Create_fails_when_empty(string? value)
    {
        Result<ClientId> result = ClientId.Create(value);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(ConnectionErrors.ClientIdEmpty);
    }

    [Fact]
    public void ClientId_Create_fails_when_too_long()
    {
        Result<ClientId> result = ClientId.Create(new string('a', ClientId.MaxLength + 1));

        result.Error.ShouldBe(ConnectionErrors.ClientIdTooLong);
    }

    [Fact]
    public void ClientId_Create_trims_and_compares_by_value()
    {
        ClientId first = ClientId.Create("  client-1 ").Value;
        ClientId second = ClientId.Create("client-1").Value;

        first.Value.ShouldBe("client-1");
        first.ShouldBe(second);
    }

    [Fact]
    public void ConnectionId_Create_fails_when_empty()
    {
        ConnectionId.Create(" ").Error.ShouldBe(ConnectionErrors.ConnectionIdEmpty);
    }

    [Fact]
    public void ConnectionId_Create_succeeds_with_valid_value()
    {
        Result<ConnectionId> result = ConnectionId.Create("abc123");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("abc123");
        result.Error.ShouldBe(Error.None);
    }
}

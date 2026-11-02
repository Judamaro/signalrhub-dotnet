namespace SignalRHub.Test.Domain;

public sealed class NotificationTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_fails_without_type(string? type)
    {
        Notification.Create(type, null, Now).Error.ShouldBe(NotificationErrors.TypeEmpty);
    }

    [Fact]
    public void Create_fails_when_type_is_too_long()
    {
        Notification.Create(new string('x', Notification.TypeMaxLength + 1), null, Now)
            .Error.ShouldBe(NotificationErrors.TypeTooLong);
    }

    [Fact]
    public void Create_keeps_payload_and_timestamp()
    {
        using JsonDocument document = JsonDocument.Parse("""{"orderId":7}""");

        Notification notification = Notification.Create("order.updated", document.RootElement, Now).Value;

        notification.Type.ShouldBe("order.updated");
        notification.CreatedAt.ShouldBe(Now);
        notification.Id.ShouldNotBe(Guid.Empty);
        notification.Payload!.Value.GetProperty("orderId").GetInt32().ShouldBe(7);
    }
}

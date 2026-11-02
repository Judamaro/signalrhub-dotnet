namespace SignalRHub.Test.Domain;

public sealed class ResultTests
{
    private static readonly Error SomeError = Error.NotFound("Some.Code", "Something was not found.");

    [Fact]
    public void Success_exposes_value()
    {
        Result<int> result = 42;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void Failure_exposes_error_and_hides_value()
    {
        Result<int> result = SomeError;

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SomeError);
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_requires_an_error()
    {
        Should.Throw<ArgumentException>(() => Result.Failure(Error.None));
    }
}

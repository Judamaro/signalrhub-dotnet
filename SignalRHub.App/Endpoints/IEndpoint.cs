namespace SignalRHub.App.Endpoints;

/// <summary>A group of minimal API routes, discovered and mapped automatically at startup.</summary>
internal interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

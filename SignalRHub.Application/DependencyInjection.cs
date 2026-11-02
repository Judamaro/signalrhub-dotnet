namespace SignalRHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        Assembly assembly = typeof(DependencyInjection).Assembly;

        services.TryAddSingleton(TimeProvider.System);

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo<INotificationDeliveryStrategy>(), publicOnly: false)
                .As<INotificationDeliveryStrategy>()
                .WithScopedLifetime());

        services.AddScoped<IDeliveryStrategyResolver, DeliveryStrategyResolver>();

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationCommandHandlerDecorator<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingCommandHandlerDecorator<,>));

        return services;
    }
}

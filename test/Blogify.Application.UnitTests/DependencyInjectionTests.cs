using Blogify.Application.Abstractions.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Blogify.Application.UnitTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_Should_RegisterMediatR()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Assert
        var mediatorDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMediator));

        mediatorDescriptor.ShouldNotBeNull();
        mediatorDescriptor.Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddApplication_Should_RegisterPipelineBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Assert
        var behaviors = services
            .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>))
            .ToList();

        behaviors.ShouldNotBeEmpty();
        behaviors.Count.ShouldBe(3);

        behaviors.ShouldContain(d => d.ImplementationType == typeof(LoggingBehavior<,>));
        behaviors.ShouldContain(d => d.ImplementationType == typeof(ValidationBehavior<,>));
        behaviors.ShouldContain(d => d.ImplementationType == typeof(QueryCachingBehavior<,>));
    }

    [Fact]
    public void AddApplication_Should_RegisterValidatorsFromAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Assert
        var validatorDescriptor = services.FirstOrDefault(d =>
            d.ServiceType.IsGenericType &&
            d.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>) &&
            d.ImplementationType != null);

        validatorDescriptor.ShouldNotBeNull(
            "No validators were found registered in the service collection. Check if AddValidatorsFromAssembly was called.");

        validatorDescriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddApplication_Should_ReturnSameServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var chainedServices = services.AddApplication();

        // Assert
        chainedServices.ShouldBeSameAs(services);
    }
}
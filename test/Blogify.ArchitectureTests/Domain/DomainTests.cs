using System.Reflection;
using Blogify.ArchitectureTests.Infrastructure;
using Blogify.Domain.Abstractions;
using NetArchTest.Rules;
using Shouldly;
using Xunit.Abstractions;

namespace Blogify.ArchitectureTests.Domain;

public class DomainTests(ITestOutputHelper testOutputHelper) : BaseTest
{
    [Fact]
    public void DomainEvents_Should_BeSealed()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .And()
            .AreNotAbstract() // Abstract types (e.g., DomainEvent base class) are excluded since they are designed for inheritance.
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void DomainEvent_ShouldHave_DomainEventPostfix()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Entities_ShouldHave_PrivateParameterlessConstructor()
    {
        // 1. Get all types that inherit from our base Entity class.
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes();

        var failingTypes = new List<Type>();

        foreach (var entityType in entityTypes)
            // 2. Distinguish between abstract base classes and concrete entities.
            if (entityType.IsAbstract)
            {
                // RULE for ABSTRACT classes: They must have a PROTECTED parameterless constructor.
                var protectedConstructors = entityType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                if (!protectedConstructors.Any(c =>
                        c.IsFamily && c.GetParameters().Length == 0)) // c.IsFamily checks for 'protected'
                    failingTypes.Add(entityType);
            }
            else
            {
                // RULE for CONCRETE classes: They must have a PRIVATE parameterless constructor.
                var privateConstructors = entityType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                if (!privateConstructors.Any(c => c.IsPrivate && c.GetParameters().Length == 0))
                    failingTypes.Add(entityType);
            }

        // 3. Assert that our list of violators is empty.
        failingTypes.ShouldBeEmpty();
    }
}
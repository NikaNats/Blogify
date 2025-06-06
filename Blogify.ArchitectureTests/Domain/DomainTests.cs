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
        TestResult result = Types.InAssembly(DomainAssembly)
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
        TestResult result = Types.InAssembly(DomainAssembly)
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
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes();

        var failingTypes = new List<Type>();
        foreach (Type entityType in entityTypes)
        {
            ConstructorInfo[] constructors = entityType.GetConstructors(BindingFlags.NonPublic |
                                                                        BindingFlags.Instance);

            if (!constructors.Any(c => c.IsPrivate && c.GetParameters().Length == 0))
            {
                failingTypes.Add(entityType);
            }
        }

        failingTypes.ShouldBeEmpty();  // Shouldly assertion
    }
}
using System.Reflection;
using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using Blogify.Infrastructure;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Blogify.ArchitectureTests.Infrastructure;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = typeof(IBaseCommand).Assembly;

    protected static readonly Assembly DomainAssembly = typeof(Entity).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;

    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}
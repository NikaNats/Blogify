using Blogify.Application.Abstractions.Messaging;
using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.UnitTests.Behaviors;

public sealed record TestRequest : IRequest<Result>, IBaseCommand;
using Blogify.Domain.Abstractions;
using MediatR;

namespace Blogify.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
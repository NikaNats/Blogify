using FluentValidation;

namespace Blogify.Application.Abstractions.Pagination;

public interface IPagedQuery
{
    int Page { get; }
    int PageSize { get; }
}

public static class Pagination
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}

public static class PagedQueryValidationExtensions
{
    /// <summary>Adds the shared page/pageSize rules to a validator.</summary>
    public static void AddPagingRules<T>(this AbstractValidator<T> validator) where T : IPagedQuery
    {
        validator.RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.");
        validator.RuleFor(x => x.PageSize).InclusiveBetween(1, Pagination.MaxPageSize)
            .WithMessage($"PageSize must be between 1 and {Pagination.MaxPageSize}.");
    }
}

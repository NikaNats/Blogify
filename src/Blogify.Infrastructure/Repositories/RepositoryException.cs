namespace Blogify.Infrastructure.Repositories;

public class RepositoryException(string message, Exception innerException) : Exception(message, innerException);
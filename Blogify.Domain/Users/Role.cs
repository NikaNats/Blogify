namespace Blogify.Domain.Users;

public sealed class Role
{
    public static readonly Role Registered = new(1, "Registered");
    public static readonly Role Administrator = new(2, "Administrator");
    public static readonly Role Premium = new(3, "Premium");
    public Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public ICollection<User> Users { get; init; } = new List<User>();

    public ICollection<Permission> Permissions { get; init; } = new List<Permission>();
}

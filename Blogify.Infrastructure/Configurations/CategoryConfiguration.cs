using Blogify.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogify.Infrastructure.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Configure the table name (optional)
        builder.ToTable("Categories");

        // Configure the primary key
        builder.HasKey(c => c.Id);

        // Configure properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200); // Adjust the max length as needed

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(1000); // Adjust the max length as needed

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.LastModifiedAt)
            .IsRequired(false); // UpdatedAt is nullable

        // Configure relationships (if any)
        builder.HasMany(c => c.Posts)
            .WithOne() // Assuming Post has a navigation property to Category
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Adjust the delete behavior as needed

        // Configure indexes (if any)
        builder.HasIndex(c => c.Name)
            .IsUnique(); // Ensure category names are unique
    }
}
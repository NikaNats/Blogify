using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogify.Infrastructure.Configurations;

internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        // Configure the table name (optional)
        builder.ToTable("Posts");

        // Configure the primary key
        builder.HasKey(p => p.Id);

        // Configure properties
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200); // Adjust the max length as needed

        builder.Property(p => p.Content)
            .IsRequired();

        builder.Property(p => p.Excerpt)
            .IsRequired()
            .HasMaxLength(500); // Adjust the max length as needed

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200); // Adjust the max length as needed

        builder.Property(p => p.AuthorId)
            .IsRequired();

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false); // UpdatedAt is nullable

        builder.Property(p => p.PublishedAt)
            .IsRequired(false); // PublishedAt is nullable

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>(); // Store the enum as a string in the database

        // Configure relationships
        builder.HasMany(p => p.Comments)
            .WithOne() // Assuming Comment has a navigation property to Post
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade); // Adjust the delete behavior as needed

        builder.HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity(j => j.ToTable("PostTags")); // Configure the join table

        builder.HasOne<Category>()
            .WithMany(c => c.Posts)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Adjust the delete behavior as needed

        // Configure indexes
        builder.HasIndex(p => p.Slug)
            .IsUnique(); // Ensure slugs are unique

        builder.HasIndex(p => p.AuthorId);
        builder.HasIndex(p => p.CategoryId);
    }
}
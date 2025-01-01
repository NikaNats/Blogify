using Blogify.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogify.Infrastructure.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        // Configure the table name (optional)
        builder.ToTable("Tags");

        // Configure the primary key
        builder.HasKey(t => t.Id);

        // Configure properties
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100); // Adjust the max length as needed

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Configure relationships (if any)
        builder.HasMany(t => t.Posts)
            .WithMany(p => p.Tags) // Assuming Post has a navigation property for Tags
            .UsingEntity(j => j.ToTable("PostTags")); // Configure the join table

        // Configure indexes (if any)
        builder.HasIndex(t => t.Name)
            .IsUnique(); // Ensure tag names are unique
    }
}
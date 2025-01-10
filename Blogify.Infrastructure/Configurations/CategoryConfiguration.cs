using Blogify.Domain.Categories;
using Blogify.Domain.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogify.Infrastructure.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(c => c.Id);

        builder.OwnsOne(c => c.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.Value)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(200)
                .HasComment("The name of the category. Must be unique.");
        });

        builder.OwnsOne(c => c.Description, descriptionBuilder =>
        {
            descriptionBuilder.Property(d => d.Value)
                .HasColumnName("description")
                .IsRequired()
                .HasMaxLength(1000)
                .HasComment("A detailed description of the category.");
        });

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasComment("The timestamp when the category was created.");

        builder.Property(c => c.LastModifiedAt)
            .IsRequired(false)
            .HasComment("The timestamp when the category was last modified.");
    }
}
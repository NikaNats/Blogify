using Blogify.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blogify.Infrastructure.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(t => t.Id);

        builder.OwnsOne(c => c.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.Value)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(200);
        });

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasComment("The timestamp when the tag was created.");

        builder.Property(t => t.LastModifiedAt)
            .IsRequired(false)
            .HasComment("The timestamp when the tag was last modified.");
    }
}
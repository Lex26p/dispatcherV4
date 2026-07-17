using Dispatcher.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations;

internal sealed class TagValueConfiguration : IEntityTypeConfiguration<TagValue>
{
    public void Configure(EntityTypeBuilder<TagValue> builder)
    {
        builder.ToTable("current_tag_values");
        builder.ConfigureEntity();

        builder.Property(value => value.TagId)
            .IsRequired();

        builder.Property(value => value.DeviceId)
            .IsRequired();

        builder.Property(value => value.Value)
            .HasMaxLength(4000);

        builder.Property(value => value.Quality)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(value => value.Timestamp)
            .IsRequired();

        builder.Property(value => value.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(value => value.TagId).IsUnique();
        builder.HasIndex(value => value.DeviceId);
        builder.HasIndex(value => value.Timestamp);
    }
}

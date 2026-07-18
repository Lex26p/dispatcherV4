using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations.Assets;

public sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations", SchemaNames.Assets);
        builder.HasKey(location => location.Id);

        builder.Property(location => location.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .ValueGeneratedNever();

        builder.Property(location => location.ParentLocationId)
            .HasColumnName("parent_location_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? EntityId.From(value.Value) : null);

        builder.Property(location => location.Code)
            .HasColumnName("code")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(location => location.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(location => location.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(location => location.IsArchived)
            .HasColumnName("is_archived")
            .IsRequired();

        builder.Property(location => location.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(location => location.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(location => location.Code).IsUnique();
        builder.HasIndex(location => location.ParentLocationId);
    }
}

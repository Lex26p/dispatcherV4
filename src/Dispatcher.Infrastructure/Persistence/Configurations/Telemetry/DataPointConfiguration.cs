using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations.Telemetry;

public sealed class DataPointConfiguration : IEntityTypeConfiguration<DataPoint>
{
    public void Configure(EntityTypeBuilder<DataPoint> builder)
    {
        builder.ToTable("data_points", SchemaNames.Telemetry);
        builder.HasKey(point => point.Id);

        builder.Property(point => point.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .ValueGeneratedNever();

        builder.Property(point => point.EquipmentId)
            .HasColumnName("equipment_id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .IsRequired();

        builder.Property(point => point.Code)
            .HasColumnName("code")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(point => point.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(point => point.ValueKind)
            .HasColumnName("value_kind")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(point => point.Unit)
            .HasColumnName("unit")
            .HasMaxLength(40);

        builder.Property(point => point.FreshnessTimeoutSeconds)
            .HasColumnName("freshness_timeout_seconds")
            .IsRequired();

        builder.Property(point => point.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(point => point.IsArchived)
            .HasColumnName("is_archived")
            .IsRequired();

        builder.Property(point => point.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(point => point.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(point => point.Code).IsUnique();
        builder.HasIndex(point => point.EquipmentId);

        builder.HasOne<Equipment>()
            .WithMany()
            .HasForeignKey(point => point.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

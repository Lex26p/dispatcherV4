using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations.Telemetry;

public sealed class ProtocolMappingConfiguration : IEntityTypeConfiguration<ProtocolMapping>
{
    public void Configure(EntityTypeBuilder<ProtocolMapping> builder)
    {
        builder.ToTable("protocol_mappings", SchemaNames.Telemetry);
        builder.HasKey(mapping => mapping.Id);

        builder.Property(mapping => mapping.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .ValueGeneratedNever();

        builder.Property(mapping => mapping.DataPointId)
            .HasColumnName("data_point_id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .IsRequired();

        builder.Property(mapping => mapping.TelemetrySourceId)
            .HasColumnName("telemetry_source_id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .IsRequired();

        builder.Property(mapping => mapping.Protocol)
            .HasColumnName("protocol")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(mapping => mapping.MappingSchemaVersion)
            .HasColumnName("mapping_schema_version")
            .IsRequired();

        builder.Property(mapping => mapping.MappingJson)
            .HasColumnName("mapping_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(mapping => mapping.IsArchived)
            .HasColumnName("is_archived")
            .IsRequired();

        builder.Property(mapping => mapping.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(mapping => mapping.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(mapping => mapping.DataPointId).IsUnique();
        builder.HasIndex(mapping => mapping.TelemetrySourceId);

        builder.HasOne<DataPoint>()
            .WithMany()
            .HasForeignKey(mapping => mapping.DataPointId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TelemetrySource>()
            .WithMany()
            .HasForeignKey(mapping => mapping.TelemetrySourceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

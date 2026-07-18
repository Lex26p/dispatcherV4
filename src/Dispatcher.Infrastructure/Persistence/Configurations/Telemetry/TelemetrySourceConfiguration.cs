using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations.Telemetry;

public sealed class TelemetrySourceConfiguration : IEntityTypeConfiguration<TelemetrySource>
{
    public void Configure(EntityTypeBuilder<TelemetrySource> builder)
    {
        builder.ToTable("telemetry_sources", SchemaNames.Telemetry);
        builder.HasKey(source => source.Id);

        builder.Property(source => source.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .ValueGeneratedNever();

        builder.Property(source => source.Code)
            .HasColumnName("code")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(source => source.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(source => source.Protocol)
            .HasColumnName("protocol")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(source => source.Endpoint)
            .HasColumnName("endpoint")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(source => source.ConfigurationSchemaVersion)
            .HasColumnName("configuration_schema_version")
            .IsRequired();

        builder.Property(source => source.ConfigurationJson)
            .HasColumnName("configuration_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(source => source.SecretReference)
            .HasColumnName("secret_reference")
            .HasMaxLength(500);

        builder.Property(source => source.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(source => source.IsEnabled)
            .HasColumnName("is_enabled")
            .IsRequired();

        builder.Property(source => source.IsArchived)
            .HasColumnName("is_archived")
            .IsRequired();

        builder.Property(source => source.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(source => source.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(source => source.Code).IsUnique();
        builder.HasIndex(source => source.Protocol);
    }
}

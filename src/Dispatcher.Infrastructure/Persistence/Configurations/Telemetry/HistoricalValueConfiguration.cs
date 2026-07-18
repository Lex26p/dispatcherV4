using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations.Telemetry;

public sealed class HistoricalValueConfiguration : IEntityTypeConfiguration<HistoricalValue>
{
    public void Configure(EntityTypeBuilder<HistoricalValue> builder)
    {
        builder.ToTable("historical_values", SchemaNames.Telemetry);
        builder.HasKey(value => value.Id);

        builder.Property(value => value.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .ValueGeneratedNever();

        builder.Property(value => value.DataPointId)
            .HasColumnName("data_point_id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .IsRequired();

        builder.Property(value => value.TelemetrySourceId)
            .HasColumnName("telemetry_source_id")
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? EntityId.From(value.Value) : null);

        builder.Property(value => value.Sequence)
            .HasColumnName("sequence")
            .IsRequired();

        builder.Property(value => value.ValueKind)
            .HasColumnName("value_kind")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(value => value.RawValue)
            .HasColumnName("raw_value")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(value => value.Unit)
            .HasColumnName("unit")
            .HasMaxLength(40);

        builder.Property(value => value.Quality)
            .HasColumnName("quality")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(value => value.SourceTimestampUtc)
            .HasColumnName("source_timestamp_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(value => value.ReceivedAtUtc)
            .HasColumnName("received_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(value => value.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(value => value.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(1000);

        builder.HasIndex(value => new { value.DataPointId, value.Sequence }).IsUnique();
        builder.HasIndex(value => new { value.DataPointId, value.SourceTimestampUtc });
        builder.HasIndex(value => value.TelemetrySourceId);
        builder.HasIndex(value => value.ReceivedAtUtc);
        builder.HasIndex(value => value.Quality);

        builder.HasOne<DataPoint>()
            .WithMany()
            .HasForeignKey(value => value.DataPointId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TelemetrySource>()
            .WithMany()
            .HasForeignKey(value => value.TelemetrySourceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

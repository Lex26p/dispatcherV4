using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;
using Dispatcher.Domain.IdentityAccess;
using Dispatcher.Domain.Telemetry;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Dispatcher.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DispatcherDbContext))]
public partial class DispatcherDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasDefaultSchema("public")
            .HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity<Location>(builder =>
        {
            builder.ToTable("locations", SchemaNames.Assets);
            builder.HasKey(location => location.Id);
            builder.Property(location => location.Id).HasColumnName("id").HasConversion(id => id.Value, value => EntityId.From(value)).ValueGeneratedNever();
            builder.Property(location => location.ParentLocationId).HasColumnName("parent_location_id").HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? EntityId.From(value.Value) : null);
            builder.Property(location => location.Code).HasColumnName("code").HasMaxLength(80).IsRequired();
            builder.Property(location => location.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(location => location.Description).HasColumnName("description").HasMaxLength(1000);
            builder.Property(location => location.IsArchived).HasColumnName("is_archived").IsRequired();
            builder.Property(location => location.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(location => location.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.HasIndex(location => location.Code).IsUnique();
            builder.HasIndex(location => location.ParentLocationId);
        });

        modelBuilder.Entity<Equipment>(builder =>
        {
            builder.ToTable("equipment", SchemaNames.Assets);
            builder.HasKey(equipment => equipment.Id);
            builder.Property(equipment => equipment.Id).HasColumnName("id").HasConversion(id => id.Value, value => EntityId.From(value)).ValueGeneratedNever();
            builder.Property(equipment => equipment.LocationId).HasColumnName("location_id").HasConversion(id => id.Value, value => EntityId.From(value)).IsRequired();
            builder.Property(equipment => equipment.Code).HasColumnName("code").HasMaxLength(80).IsRequired();
            builder.Property(equipment => equipment.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(equipment => equipment.Description).HasColumnName("description").HasMaxLength(1000);
            builder.Property(equipment => equipment.IsArchived).HasColumnName("is_archived").IsRequired();
            builder.Property(equipment => equipment.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(equipment => equipment.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.HasIndex(equipment => equipment.Code).IsUnique();
            builder.HasIndex(equipment => equipment.LocationId);
            builder.HasOne<Location>().WithMany().HasForeignKey(equipment => equipment.LocationId).OnDelete(DeleteBehavior.Restrict);
        });



        modelBuilder.Entity<TelemetrySource>(builder =>
        {
            builder.ToTable("telemetry_sources", SchemaNames.Telemetry);
            builder.HasKey(source => source.Id);
            builder.Property(source => source.Id).HasColumnName("id").HasConversion(id => id.Value, value => EntityId.From(value)).ValueGeneratedNever();
            builder.Property(source => source.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
            builder.Property(source => source.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(source => source.Protocol).HasColumnName("protocol").HasConversion<int>().IsRequired();
            builder.Property(source => source.Endpoint).HasColumnName("endpoint").HasMaxLength(500).IsRequired();
            builder.Property(source => source.ConfigurationSchemaVersion).HasColumnName("configuration_schema_version").IsRequired();
            builder.Property(source => source.ConfigurationJson).HasColumnName("configuration_json").HasColumnType("jsonb").IsRequired();
            builder.Property(source => source.SecretReference).HasColumnName("secret_reference").HasMaxLength(500);
            builder.Property(source => source.Description).HasColumnName("description").HasMaxLength(1000);
            builder.Property(source => source.IsEnabled).HasColumnName("is_enabled").IsRequired();
            builder.Property(source => source.IsArchived).HasColumnName("is_archived").IsRequired();
            builder.Property(source => source.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(source => source.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.HasIndex(source => source.Code).IsUnique();
            builder.HasIndex(source => source.Protocol);
        });

        modelBuilder.Entity<DataPoint>(builder =>
        {
            builder.ToTable("data_points", SchemaNames.Telemetry);
            builder.HasKey(point => point.Id);
            builder.Property(point => point.Id).HasColumnName("id").HasConversion(id => id.Value, value => EntityId.From(value)).ValueGeneratedNever();
            builder.Property(point => point.EquipmentId).HasColumnName("equipment_id").HasConversion(id => id.Value, value => EntityId.From(value)).IsRequired();
            builder.Property(point => point.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
            builder.Property(point => point.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(point => point.ValueKind).HasColumnName("value_kind").HasConversion<int>().IsRequired();
            builder.Property(point => point.Unit).HasColumnName("unit").HasMaxLength(40);
            builder.Property(point => point.FreshnessTimeoutSeconds).HasColumnName("freshness_timeout_seconds").IsRequired();
            builder.Property(point => point.Description).HasColumnName("description").HasMaxLength(1000);
            builder.Property(point => point.IsArchived).HasColumnName("is_archived").IsRequired();
            builder.Property(point => point.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(point => point.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.HasIndex(point => point.Code).IsUnique();
            builder.HasIndex(point => point.EquipmentId);
            builder.HasOne<Equipment>().WithMany().HasForeignKey(point => point.EquipmentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProtocolMapping>(builder =>
        {
            builder.ToTable("protocol_mappings", SchemaNames.Telemetry);
            builder.HasKey(mapping => mapping.Id);
            builder.Property(mapping => mapping.Id).HasColumnName("id").HasConversion(id => id.Value, value => EntityId.From(value)).ValueGeneratedNever();
            builder.Property(mapping => mapping.DataPointId).HasColumnName("data_point_id").HasConversion(id => id.Value, value => EntityId.From(value)).IsRequired();
            builder.Property(mapping => mapping.TelemetrySourceId).HasColumnName("telemetry_source_id").HasConversion(id => id.Value, value => EntityId.From(value)).IsRequired();
            builder.Property(mapping => mapping.Protocol).HasColumnName("protocol").HasConversion<int>().IsRequired();
            builder.Property(mapping => mapping.MappingSchemaVersion).HasColumnName("mapping_schema_version").IsRequired();
            builder.Property(mapping => mapping.MappingJson).HasColumnName("mapping_json").HasColumnType("jsonb").IsRequired();
            builder.Property(mapping => mapping.IsArchived).HasColumnName("is_archived").IsRequired();
            builder.Property(mapping => mapping.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(mapping => mapping.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.HasIndex(mapping => mapping.DataPointId).IsUnique();
            builder.HasIndex(mapping => mapping.TelemetrySourceId);
            builder.HasOne<DataPoint>().WithMany().HasForeignKey(mapping => mapping.DataPointId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<TelemetrySource>().WithMany().HasForeignKey(mapping => mapping.TelemetrySourceId).OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<CurrentValue>(builder =>
        {
            builder.ToTable("current_values", SchemaNames.Telemetry);
            builder.HasKey(value => value.DataPointId);
            builder.Property(value => value.DataPointId).HasColumnName("data_point_id").HasConversion(id => id.Value, value => EntityId.From(value)).ValueGeneratedNever();
            builder.Property(value => value.TelemetrySourceId).HasColumnName("telemetry_source_id").HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? EntityId.From(value.Value) : null);
            builder.Property(value => value.Sequence).HasColumnName("sequence").IsRequired();
            builder.Property(value => value.ValueKind).HasColumnName("value_kind").HasConversion<int>().IsRequired();
            builder.Property(value => value.RawValue).HasColumnName("raw_value").HasMaxLength(2000).IsRequired();
            builder.Property(value => value.Unit).HasColumnName("unit").HasMaxLength(40);
            builder.Property(value => value.Quality).HasColumnName("quality").HasConversion<int>().IsRequired();
            builder.Property(value => value.SourceTimestampUtc).HasColumnName("source_timestamp_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(value => value.ReceivedAtUtc).HasColumnName("received_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(value => value.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(value => value.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
            builder.HasIndex(value => value.TelemetrySourceId);
            builder.HasIndex(value => value.Quality);
            builder.HasIndex(value => value.ReceivedAtUtc);
            builder.HasOne<DataPoint>().WithMany().HasForeignKey(value => value.DataPointId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<TelemetrySource>().WithMany().HasForeignKey(value => value.TelemetrySourceId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<HistoricalValue>(builder =>
        {
            builder.ToTable("historical_values", SchemaNames.Telemetry);
            builder.HasKey(value => value.Id);
            builder.Property(value => value.Id).HasColumnName("id").HasConversion(id => id.Value, value => EntityId.From(value)).ValueGeneratedNever();
            builder.Property(value => value.DataPointId).HasColumnName("data_point_id").HasConversion(id => id.Value, value => EntityId.From(value)).IsRequired();
            builder.Property(value => value.TelemetrySourceId).HasColumnName("telemetry_source_id").HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? EntityId.From(value.Value) : null);
            builder.Property(value => value.Sequence).HasColumnName("sequence").IsRequired();
            builder.Property(value => value.ValueKind).HasColumnName("value_kind").HasConversion<int>().IsRequired();
            builder.Property(value => value.RawValue).HasColumnName("raw_value").HasMaxLength(2000).IsRequired();
            builder.Property(value => value.Unit).HasColumnName("unit").HasMaxLength(40);
            builder.Property(value => value.Quality).HasColumnName("quality").HasConversion<int>().IsRequired();
            builder.Property(value => value.SourceTimestampUtc).HasColumnName("source_timestamp_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(value => value.ReceivedAtUtc).HasColumnName("received_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(value => value.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(value => value.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
            builder.HasIndex(value => new { value.DataPointId, value.Sequence }).IsUnique();
            builder.HasIndex(value => new { value.DataPointId, value.SourceTimestampUtc });
            builder.HasIndex(value => value.TelemetrySourceId);
            builder.HasIndex(value => value.ReceivedAtUtc);
            builder.HasIndex(value => value.Quality);
            builder.HasOne<DataPoint>().WithMany().HasForeignKey(value => value.DataPointId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<TelemetrySource>().WithMany().HasForeignKey(value => value.TelemetrySourceId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserAccount>(builder =>
        {
            builder.ToTable("user_accounts", SchemaNames.Identity);
            builder.HasKey(user => user.Id);
            builder.Property(user => user.Id).HasColumnName("id").HasConversion(id => id.Value, value => EntityId.From(value)).ValueGeneratedNever();
            builder.Property(user => user.ExternalId).HasColumnName("external_id").HasMaxLength(160).IsRequired();
            builder.Property(user => user.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();
            builder.Property(user => user.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
            builder.Property(user => user.IsActive).HasColumnName("is_active").IsRequired();
            builder.Property(user => user.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.HasIndex(user => user.ExternalId).IsUnique();
            builder.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<Role>(builder =>
        {
            builder.ToTable("roles", SchemaNames.Identity);
            builder.HasKey(role => role.Id);
            builder.Property(role => role.Id).HasColumnName("id").HasConversion(id => id.Value, value => EntityId.From(value)).ValueGeneratedNever();
            builder.Property(role => role.Code).HasColumnName("code").HasMaxLength(120).IsRequired();
            builder.Property(role => role.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(role => role.Description).HasColumnName("description").HasMaxLength(1000);
            builder.Property(role => role.PermissionsText).HasColumnName("permissions").IsRequired();
            builder.Property(role => role.IsSystem).HasColumnName("is_system").IsRequired();
            builder.Property(role => role.IsArchived).HasColumnName("is_archived").IsRequired();
            builder.Ignore(role => role.PermissionSet);
            builder.HasIndex(role => role.Code).IsUnique();
        });

        modelBuilder.Entity<PermissionScope>(builder =>
        {
            builder.ToTable("permission_scopes", SchemaNames.Identity);
            builder.HasKey(scope => scope.Id);
            builder.Property(scope => scope.Id).HasColumnName("id").HasConversion(id => id.Value, value => EntityId.From(value)).ValueGeneratedNever();
            builder.Property(scope => scope.Code).HasColumnName("code").HasMaxLength(120).IsRequired();
            builder.Property(scope => scope.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(scope => scope.Description).HasColumnName("description").HasMaxLength(1000);
            builder.Property(scope => scope.IsArchived).HasColumnName("is_archived").IsRequired();
            builder.Property(scope => scope.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.HasIndex(scope => scope.Code).IsUnique();
        });

        modelBuilder.Entity<RoleAssignment>(builder =>
        {
            builder.ToTable("role_assignments", SchemaNames.Identity);
            builder.HasKey(assignment => assignment.Id);
            builder.Property(assignment => assignment.Id).HasColumnName("id").HasConversion(id => id.Value, value => EntityId.From(value)).ValueGeneratedNever();
            builder.Property(assignment => assignment.UserId).HasColumnName("user_id").HasConversion(id => id.Value, value => EntityId.From(value)).IsRequired();
            builder.Property(assignment => assignment.RoleId).HasColumnName("role_id").HasConversion(id => id.Value, value => EntityId.From(value)).IsRequired();
            builder.Property(assignment => assignment.ScopeId).HasColumnName("scope_id").HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? EntityId.From(value.Value) : null);
            builder.Property(assignment => assignment.Source).HasColumnName("source").HasMaxLength(120).IsRequired();
            builder.Property(assignment => assignment.Reason).HasColumnName("reason").HasMaxLength(1000);
            builder.Property(assignment => assignment.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(assignment => assignment.RevokedAtUtc).HasColumnName("revoked_at_utc").HasColumnType("timestamp with time zone");
            builder.Property(assignment => assignment.RevokedReason).HasColumnName("revoked_reason").HasMaxLength(1000);
            builder.HasIndex(assignment => new { assignment.UserId, assignment.RoleId, assignment.ScopeId });
        });
#pragma warning restore 612, 618
    }
}

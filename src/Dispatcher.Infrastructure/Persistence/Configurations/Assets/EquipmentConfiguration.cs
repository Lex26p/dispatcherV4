using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations.Assets;

public sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("equipment", SchemaNames.Assets);
        builder.HasKey(equipment => equipment.Id);

        builder.Property(equipment => equipment.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .ValueGeneratedNever();

        builder.Property(equipment => equipment.LocationId)
            .HasColumnName("location_id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .IsRequired();

        builder.Property(equipment => equipment.Code)
            .HasColumnName("code")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(equipment => equipment.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(equipment => equipment.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(equipment => equipment.IsArchived)
            .HasColumnName("is_archived")
            .IsRequired();

        builder.Property(equipment => equipment.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(equipment => equipment.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(equipment => equipment.Code).IsUnique();
        builder.HasIndex(equipment => equipment.LocationId);

        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(equipment => equipment.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using Dispatcher.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");
        builder.ConfigureEntity();

        builder.Property(tag => tag.DeviceId)
            .IsRequired();

        builder.Property(tag => tag.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(tag => tag.Code)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(tag => tag.SourceType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(tag => tag.Protocol)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(tag => tag.DataType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(tag => tag.Unit)
            .HasMaxLength(50);

        builder.Property(tag => tag.Scale)
            .IsRequired();

        builder.Property(tag => tag.Offset)
            .IsRequired();

        builder.Property(tag => tag.PollIntervalMs)
            .IsRequired();

        builder.Property(tag => tag.IsEnabled)
            .IsRequired();

        builder.Property(tag => tag.HistoryEnabled)
            .IsRequired();

        builder.Property(tag => tag.Description)
            .HasMaxLength(1000);

        builder.OwnsOne(tag => tag.ModbusAddress, owned =>
        {
            owned.Property(address => address.RegisterType)
                .HasColumnName("modbus_register_type")
                .HasConversion<int>();

            owned.Property(address => address.Address)
                .HasColumnName("modbus_address");

            owned.Property(address => address.UnitId)
                .HasColumnName("modbus_unit_id");

            owned.Property(address => address.ByteOrder)
                .HasColumnName("modbus_byte_order")
                .HasConversion<int>();

            owned.Property(address => address.WordOrder)
                .HasColumnName("modbus_word_order")
                .HasConversion<int>();
        });

        builder.OwnsOne(tag => tag.SnmpAddress, owned =>
        {
            owned.Property(address => address.Oid)
                .HasColumnName("snmp_oid")
                .HasMaxLength(500);
        });

        builder.HasIndex(tag => tag.DeviceId);
        builder.HasIndex(tag => tag.Code).IsUnique();
    }
}

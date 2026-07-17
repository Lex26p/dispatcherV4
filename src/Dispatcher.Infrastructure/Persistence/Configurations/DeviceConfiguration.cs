using Dispatcher.Domain.Devices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations;

internal sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");
        builder.ConfigureEntity();

        builder.Property(device => device.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(device => device.Protocol)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(device => device.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(device => device.IsEnabled)
            .IsRequired();

        builder.Property(device => device.Description)
            .HasMaxLength(1000);

        builder.OwnsOne(device => device.ConnectionSettings, owned =>
        {
            owned.Property(settings => settings.Host)
                .HasColumnName("host")
                .HasMaxLength(255)
                .IsRequired();

            owned.Property(settings => settings.Port)
                .HasColumnName("port")
                .IsRequired();

            owned.Property(settings => settings.PollIntervalMs)
                .HasColumnName("poll_interval_ms")
                .IsRequired();

            owned.Property(settings => settings.TimeoutMs)
                .HasColumnName("timeout_ms")
                .IsRequired();

            owned.Property(settings => settings.RetryCount)
                .HasColumnName("retry_count")
                .IsRequired();

            owned.Property(settings => settings.SnmpVersion)
                .HasColumnName("snmp_version")
                .HasConversion<int?>();

            owned.Property(settings => settings.SnmpCommunity)
                .HasColumnName("snmp_community")
                .HasMaxLength(255);
        });

        builder.Navigation(device => device.ConnectionSettings).IsRequired();
        builder.HasIndex(device => device.Name);
    }
}

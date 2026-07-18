using Dispatcher.Domain.Common;
using Dispatcher.Domain.IdentityAccess;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations.Identity;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles", SchemaNames.Identity);
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .ValueGeneratedNever();
        builder.Property(role => role.Code).HasColumnName("code").HasMaxLength(120).IsRequired();
        builder.Property(role => role.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(role => role.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(role => role.PermissionsText).HasColumnName("permissions").IsRequired();
        builder.Property(role => role.IsSystem).HasColumnName("is_system").IsRequired();
        builder.Property(role => role.IsArchived).HasColumnName("is_archived").IsRequired();
        builder.Ignore(role => role.PermissionSet);
        builder.HasIndex(role => role.Code).IsUnique();
    }
}

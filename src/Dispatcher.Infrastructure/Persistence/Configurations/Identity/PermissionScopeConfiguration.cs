using Dispatcher.Domain.Common;
using Dispatcher.Domain.IdentityAccess;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations.Identity;

public sealed class PermissionScopeConfiguration : IEntityTypeConfiguration<PermissionScope>
{
    public void Configure(EntityTypeBuilder<PermissionScope> builder)
    {
        builder.ToTable("permission_scopes", SchemaNames.Identity);
        builder.HasKey(scope => scope.Id);
        builder.Property(scope => scope.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .ValueGeneratedNever();
        builder.Property(scope => scope.Code).HasColumnName("code").HasMaxLength(120).IsRequired();
        builder.Property(scope => scope.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(scope => scope.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(scope => scope.IsArchived).HasColumnName("is_archived").IsRequired();
        builder.Property(scope => scope.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(scope => scope.Code).IsUnique();
    }
}

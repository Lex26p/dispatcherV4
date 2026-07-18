using Dispatcher.Domain.Common;
using Dispatcher.Domain.IdentityAccess;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("user_accounts", SchemaNames.Identity);
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .ValueGeneratedNever();
        builder.Property(user => user.ExternalId).HasColumnName("external_id").HasMaxLength(160).IsRequired();
        builder.Property(user => user.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();
        builder.Property(user => user.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(user => user.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(user => user.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(user => user.ExternalId).IsUnique();
        builder.HasIndex(user => user.Email).IsUnique();
    }
}

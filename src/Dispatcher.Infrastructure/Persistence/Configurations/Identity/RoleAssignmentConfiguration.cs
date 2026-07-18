using Dispatcher.Domain.Common;
using Dispatcher.Domain.IdentityAccess;
using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dispatcher.Infrastructure.Persistence.Configurations.Identity;

public sealed class RoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment>
{
    public void Configure(EntityTypeBuilder<RoleAssignment> builder)
    {
        builder.ToTable("role_assignments", SchemaNames.Identity);
        builder.HasKey(assignment => assignment.Id);
        builder.Property(assignment => assignment.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .ValueGeneratedNever();
        builder.Property(assignment => assignment.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .IsRequired();
        builder.Property(assignment => assignment.RoleId)
            .HasColumnName("role_id")
            .HasConversion(id => id.Value, value => EntityId.From(value))
            .IsRequired();
        builder.Property(assignment => assignment.ScopeId)
            .HasColumnName("scope_id")
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null, value => value.HasValue ? EntityId.From(value.Value) : null);
        builder.Property(assignment => assignment.Source).HasColumnName("source").HasMaxLength(120).IsRequired();
        builder.Property(assignment => assignment.Reason).HasColumnName("reason").HasMaxLength(1000);
        builder.Property(assignment => assignment.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(assignment => assignment.RevokedAtUtc).HasColumnName("revoked_at_utc").HasColumnType("timestamp with time zone");
        builder.Property(assignment => assignment.RevokedReason).HasColumnName("revoked_reason").HasMaxLength(1000);
        builder.HasIndex(assignment => new { assignment.UserId, assignment.RoleId, assignment.ScopeId });
    }
}

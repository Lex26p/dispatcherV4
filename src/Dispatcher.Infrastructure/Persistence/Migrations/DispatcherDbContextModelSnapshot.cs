using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;
using Dispatcher.Domain.IdentityAccess;
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

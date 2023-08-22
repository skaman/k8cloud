using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace K8Cloud.Shared.Database;

/// <summary>
/// Base class for the database context.
/// It's used as an empty base context that is populated with the entities from the modules.
/// </summary>
public class K8CloudDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the build action.
    /// The build actions is called during the <see cref="OnModelCreating(ModelBuilder)"/> method.
    /// </summary>
    internal static Action<ModelBuilder>? BuildAction { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="K8CloudDbContext"/> class.
    /// </summary>
    /// <param name="options">DB conctext options.</param>
    public K8CloudDbContext(DbContextOptions<K8CloudDbContext> options) : base(options) { }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        BuildAction?.Invoke(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AddTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        AddTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Update timestamps to the added and modified entities.
    /// </summary>
    private void AddTimestamps()
    {
        var entities = ChangeTracker
            .Entries()
            .Where(
                x =>
                    x.Entity is Entity
                    && (x.State == EntityState.Added || x.State == EntityState.Modified)
            );

        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            if (entity.State == EntityState.Added)
            {
                ((Entity)entity.Entity).CreatedAt = now;
            }
            ((Entity)entity.Entity).UpdatedAt = now;
        }
    }
}

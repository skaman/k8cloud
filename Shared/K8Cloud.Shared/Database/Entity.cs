using System.ComponentModel.DataAnnotations;

namespace K8Cloud.Shared.Database;

/// <summary>
/// Base entity.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Entity ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Created at.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Updated at.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Entity version.
    /// </summary>
    [Timestamp]
    public uint Version { get; set; }
}

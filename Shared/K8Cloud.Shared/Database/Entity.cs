using HotChocolate.Types;
using System.ComponentModel.DataAnnotations;

namespace K8Cloud.Shared.Database;

public abstract class Entity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [Timestamp]
    public uint Version { get; set; }
}

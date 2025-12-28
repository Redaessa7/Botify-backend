namespace Botify.Domain.Models;
public abstract class AuditableEntity : Entity
{
    protected AuditableEntity()
    { }

    protected AuditableEntity(Guid id)
        : base(id)
    {
    }

    public DateTime CreatedAtUtc { get; set; } 

    public string? CreatedBy { get; set; }

    public DateTime LastModifiedUtc { get; set; }

    public string? LastModifiedBy { get; set; }
}
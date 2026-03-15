namespace Botify.Domain.Models;
public abstract class AuditableEntity : Entity
{
    protected AuditableEntity()
    { }

    protected AuditableEntity(Guid id)
        : base(id)
    {
    }
    // Soft delete fields
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get;  set; } 
    public string? DeletedBy { get;  set; } 

    public DateTime CreatedAtUtc { get; set; } 

    public string? CreatedBy { get; set; }

    public DateTime LastModifiedUtc { get; set; }

    public string? LastModifiedBy { get; set; }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
namespace Datapac.Interfaces;

public interface ISoftDelete
{
    public bool IsDeleted { get; set; }
    public DateOnly? DeletedAt { get; set; }
    
    public void Undo()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}
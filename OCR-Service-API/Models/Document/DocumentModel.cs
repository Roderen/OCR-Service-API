namespace Task_Manager_API.Models.Document;

public class DocumentModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public DateOnly BirthDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string? DocumentId { get; set; }
    public string DocumentIdHash { get; set; } = string.Empty;
}
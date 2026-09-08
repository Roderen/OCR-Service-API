namespace Task_Manager_API.DTO.Document;

public record BackSideDocumentDto(
    string Name,
    DateOnly BirthDate,
    DateOnly ExpiryDate,
    string DocumentNumber
);
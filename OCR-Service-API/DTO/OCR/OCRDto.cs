namespace Task_Manager_API.DTO.OCR;

public record OcrResultDto(
    string Text,
    List<OcrItemDto> Items,
    double SummaryScore
);

public record OcrItemDto(
    string Text,
    double Score
);
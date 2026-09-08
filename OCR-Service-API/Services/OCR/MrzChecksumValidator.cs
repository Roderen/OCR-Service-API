namespace Task_Manager_API.Services.OCR;

public static class MrzChecksumValidator
{
    private static int GetCharValue(char c)
    {
        if (c == '<') return 0;
        if (char.IsDigit(c)) return c - '0';
        if (char.IsUpper(c)) return c - 'A' + 10;
        throw new ArgumentException($"Invalid MRZ character: {c}");
    }

    public static int ComputeCheckDigit(string field)
    {
        int[] weights = { 7, 3, 1 };
        int sum = 0;
        for (int i = 0; i < field.Length; i++)
        {
            sum += GetCharValue(field[i]) * weights[i % 3];
        }
        return sum % 10;
    }

    public static bool ValidateField(string field, char expectedCheckDigit)
    {
        var computed = ComputeCheckDigit(field);
        var expected = expectedCheckDigit == '<' ? 0 : expectedCheckDigit - '0';
        return computed == expected;
    }
}
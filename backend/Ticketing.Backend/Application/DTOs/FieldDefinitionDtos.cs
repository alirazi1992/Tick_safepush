namespace Ticketing.Backend.Application.DTOs;

public class FieldOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class CreateFieldDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Type { get; set; } = "Text"; // String representation of FieldType enum
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public List<FieldOption>? Options { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
}

public class UpdateFieldDefinitionRequest
{
    public string? Name { get; set; }
    public string? Label { get; set; }
    public string? Key { get; set; }
    public string? Type { get; set; } // String representation of FieldType enum
    public bool? IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public List<FieldOption>? Options { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
}

public class FieldDefinitionResponse
{
    public int Id { get; set; }
    public int SubcategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Type { get; set; } = "Text"; // String representation of FieldType enum
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public List<FieldOption>? Options { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
}

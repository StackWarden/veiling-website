namespace backend.Db;

public class CreateSpeciesDto
{
    public string Title { get; set; } = string.Empty;
    public string? LatinName { get; set; }
    public string? Family { get; set; }
    public string? GrowthType { get; set; }
    public string? Description { get; set; }
    public bool IsPerennial { get; set; }
}

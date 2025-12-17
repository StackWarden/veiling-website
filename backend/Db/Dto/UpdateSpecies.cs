namespace backend.Db;

public class UpdateSpeciesDto
{
    public string? Title { get; set; }
    public string? LatinName { get; set; }
    public string? Family { get; set; }
    public string? GrowthType { get; set; }
    public string? Description { get; set; }
    public bool? IsPerennial { get; set; }
}
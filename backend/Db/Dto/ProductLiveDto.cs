namespace backend.Dtos
{
    // Dit DTO bevat beperkte informatie over een product in de veiling (genoeg voor de live veilingweergave).
    // We hebben een Id, de Title (meestal de soortnaam van de plant), een PhotoUrl (als er een foto is),
    // de Latijnse naam van de soort (Species), en nog wat details zoals de steel-lengte (StemLength),
    // de hoeveelheid (Quantity), de minimumprijs (MinPrice) en de potmaat (PotSize).
    // Dit zou voldoende moeten zijn om kopers te informeren over wat er geveild wordt.
    public class ProductLiveDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "Plant";
        public string? PhotoUrl { get; set; }
        public string Species { get; set; } = "Plant";
        public int StemLength { get; set; }
        public int Quantity { get; set; }
        public decimal MinPrice { get; set; }
        public string PotSize { get; set; } = "Unknown";
    }
}

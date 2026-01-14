namespace backend.Dtos
{
    /*
     * Dit DTO bevat alle gegevens om een nieuwe veiling (met items) aan te maken.
     * We hebben de AuctionneerId (degene die de veiling organiseert), de start- en eindtijd (eindtijd moet na de start liggen, vanzelfsprekend),
     * een omschrijving van de veiling, de status (standaard 'draft', want je wilt 'm waarschijnlijk niet meteen live zetten),
     * en een lijst van ProductIds die geveild gaan worden.
     * (PS: Een lege lijst betekent een veiling zonder items - niet erg spannend, maar technisch mogelijk.)
     */
    public class CreateAuctionWithItemsDto
    {
        public Guid AuctionneerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public List<Guid> ProductIds { get; set; } = new();
    }
}

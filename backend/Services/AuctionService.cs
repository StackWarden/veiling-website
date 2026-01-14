using backend.Db;
using backend.Db.Entities;
using backend.Dtos;
using Microsoft.EntityFrameworkCore;


namespace backend.Services
{
    public class AuctionService
    {
        private readonly AppDbContext _db;

        public AuctionService(AppDbContext db)
        {
            /*
                Deze service handelt alle gewone veilingszaken af (CRUD).
                Door deze logica hierheen te verplaatsen, houden we de controller schoon.
                We kunnen het hier ook makkelijk unit-testen (als we daar zin in hebben).
            */
            _db = db;
        }

        // Haalt alle veilingen op uit de database (simpele zaak, geen poespas).
        public List<AuctionDto> GetAllAuctions()
        {
            var auctions = _db.Auctions
                .Include(a => a.ClockLocation)
                .ToList();
            // Map de Auction entiteiten naar AuctionDto's (zodat we niet per ongeluk te veel info lekken).
            var result = auctions.Select(a => new AuctionDto 
            {
                Id = a.Id,
                Description = a.Description,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                ClockLocationId = a.ClockLocationId,
                ClockLocationName = a.ClockLocation?.Name,
                Items = new List<AuctionItemDto>() // Items laten we leeg hier om het simpel te houden
            }).ToList();
            return result;
        }

        // Haalt één veiling op op basis van ID.
        // Retourneert null als deze niet bestaat (de controller maakt er dan een NotFound van).
        public AuctionDto? GetAuctionById(Guid id)
        {
            var auction = _db.Auctions
                .Include(a => a.ClockLocation)
                .FirstOrDefault(a => a.Id == id);
            if (auction == null) return null;
            // Geen items opgehaald voor eenvoud. Als je items nodig hebt, kun je hier een include toevoegen.
            return new AuctionDto
            {
                Id = auction.Id,
                Description = auction.Description,
                StartTime = auction.StartTime,
                EndTime = auction.EndTime,
                Status = auction.Status,
                ClockLocationId = auction.ClockLocationId,
                ClockLocationName = auction.ClockLocation?.Name,
                Items = new List<AuctionItemDto>()
            };
        }

        // Maakt een nieuwe veiling aan met de gegeven gegevens en product-IDs.
        // Controleert of de tijden geldig zijn (tijdreizen is nog steeds niet mogelijk in EF Core).
        // Controleert daarna of alle meegegeven producten bestaan (we verkopen geen spookproducten).
        // Voegt de veiling en items toe aan de database en geeft de nieuwe veiling (met items) terug.
        public AuctionDto CreateAuction(CreateAuctionWithItemsDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException("Request body is required.");
            }
            if (dto.EndTime <= dto.StartTime)
            {
                throw new ArgumentException("End time must be after start time.");
            }

            // Lege lijst als er geen items zijn (je kunt een veiling zonder items maken, maar of dat zinvol is...).
            var productIds = dto.ProductIds ?? new List<Guid>();

            // Validate clock location if provided
            if (dto.ClockLocationId.HasValue)
            {
                if (!_db.ClockLocations.Any(cl => cl.Id == dto.ClockLocationId.Value))
                {
                    throw new ArgumentException($"Clock location {dto.ClockLocationId.Value} does not exist.");
                }
            }

            // Maak de Auction entity aan
            var auction = new Auction
            {
                Id = Guid.NewGuid(),
                AuctionneerId = dto.AuctionneerId,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = dto.Status,
                ClockLocationId = dto.ClockLocationId
            };

            _db.Auctions.Add(auction);
            _db.SaveChanges(); // Sla op om het Auction object een ID te geven in de database

            // Maak AuctionItem entries voor elk productId
            foreach (var productId in productIds)
            {
                if (!_db.Products.Any(p => p.Id == productId))
                {
                    // Oeps, een product bestaat niet
                    throw new ArgumentException($"Product {productId} does not exist.");
                }

                var auctionItem = new AuctionItem
                {
                    Id = Guid.NewGuid(),
                    AuctionId = auction.Id,
                    ProductId = productId,
                    Status = AuctionItemStatus.Pending
                };
                _db.AuctionItems.Add(auctionItem);
            }

            _db.SaveChanges();

            // Reload auction with clock location
            var createdAuction = _db.Auctions
                .Include(a => a.ClockLocation)
                .FirstOrDefault(a => a.Id == auction.Id);

            // Stel het resultaat samen met de veilinggegevens en de lijst van items
            var items = _db.AuctionItems
                .Where(ai => ai.AuctionId == auction.Id)
                .Select(ai => new AuctionItemDto
                {
                    Id = ai.Id,
                    ProductId = ai.ProductId,
                    Status = ai.Status.ToString()
                })
                .ToList();

            return new AuctionDto
            {
                Id = auction.Id,
                Description = auction.Description,
                StartTime = auction.StartTime,
                EndTime = auction.EndTime,
                Status = auction.Status,
                ClockLocationId = createdAuction?.ClockLocationId,
                ClockLocationName = createdAuction?.ClockLocation?.Name,
                Items = items
            };
        }

        // Werkt een bestaande veiling bij met nieuwe gegevens.
        // Gooit een exceptie als de veiling niet bestaat (we kunnen niks updaten wat er niet is).
        // Gooit ook een exceptie als de eindtijd vóór de starttijd ligt (nice try, maar nee).
        public string UpdateAuction(Guid id, CreateAuctionWithItemsDto dto)
        {
            var auction = _db.Auctions.Find(id);
            if (auction == null)
            {
                throw new KeyNotFoundException("Auction not found.");
            }
            if (dto.EndTime <= dto.StartTime)
            {
                throw new ArgumentException("End time must be after start time.");
            }

            // Validate clock location if provided
            if (dto.ClockLocationId.HasValue)
            {
                if (!_db.ClockLocations.Any(cl => cl.Id == dto.ClockLocationId.Value))
                {
                    throw new ArgumentException($"Clock location {dto.ClockLocationId.Value} does not exist.");
                }
            }

            auction.StartTime = dto.StartTime;
            auction.EndTime = dto.EndTime;
            auction.Status = dto.Status;
            auction.AuctionneerId = dto.AuctionneerId;
            auction.ClockLocationId = dto.ClockLocationId;

            _db.SaveChanges();

            return $"Auction {auction.Id} updated successfully.";
        }

        // Verwijdert een veiling uit de database.
        // Gooit een exceptie als de veiling niet bestaat (het heeft geen zin iets te verwijderen wat er niet is).
        // Anders verwijderen we de veiling en slaan we dat op (digitale equivalent van "weg is weg").
        public string DeleteAuction(Guid id)
        {
            var auction = _db.Auctions.Find(id);
            if (auction == null)
            {
                throw new KeyNotFoundException("Auction not found.");
            }
            _db.Auctions.Remove(auction);
            _db.SaveChanges();

            return $"Auction {auction.Id} deleted successfully.";
        }
    }
}

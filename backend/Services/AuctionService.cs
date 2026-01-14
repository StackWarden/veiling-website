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
            // Map de Auction entiteiten naar AuctionDto's (zodat we niet per ongeluk te veel info lekken).
            var auctions = _db.Auctions
                .Include(a => a.ClockLocation)
                .ToList();
            var result = auctions.Select(a => new AuctionDto
            {
                Id = a.Id,
                Description = a.Description,
                AuctionDate = a.AuctionDate,
                AuctionTime = a.AuctionTime,
                Status = a.Status,
                ClockLocationId = a.ClockLocationId,
                ClockLocationName = a.ClockLocation?.Name,
                Items = new List<AuctionItemDto>()
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

            return new AuctionDto
            {
                Id = auction.Id,
                Description = auction.Description,
                AuctionDate = auction.AuctionDate,
                AuctionTime = auction.AuctionTime,
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

            if (dto.AuctionDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new ArgumentException("Auction date cannot be in the past.");
            }

            var productIds = dto.ProductIds ?? new List<Guid>();

            // Validate clock location if provided
            if (dto.ClockLocationId.HasValue)
            {
                if (!_db.ClockLocations.Any(cl => cl.Id == dto.ClockLocationId.Value))
                {
                    throw new ArgumentException($"Clock location {dto.ClockLocationId.Value} does not exist.");
                }
            }

            var auction = new Auction
            {
                Id = Guid.NewGuid(),
                AuctionneerId = dto.AuctionneerId,
                Description = dto.Description,
                AuctionDate = dto.AuctionDate,
                AuctionTime = dto.AuctionTime,
                Status = dto.Status,
                ClockLocationId = dto.ClockLocationId
            };

            _db.Auctions.Add(auction);
            _db.SaveChanges();

            foreach (var productId in productIds)
            {
                if (!_db.Products.Any(p => p.Id == productId))
                {
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
                Status = auction.Status,
                ClockLocationId = createdAuction?.ClockLocationId,
                ClockLocationName = createdAuction?.ClockLocation?.Name,
                AuctionDate = auction.AuctionDate,
                AuctionTime = auction.AuctionTime,
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

            if (dto == null)
            {
                throw new ArgumentException("Request body is required.");
            }

            if (dto.AuctionDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new ArgumentException("Auction date cannot be in the past.");
            }

            // Validate clock location if provided
            if (dto.ClockLocationId.HasValue)
            {
                if (!_db.ClockLocations.Any(cl => cl.Id == dto.ClockLocationId.Value))
                {
                    throw new ArgumentException($"Clock location {dto.ClockLocationId.Value} does not exist.");
                }
            }

            auction.AuctionDate = dto.AuctionDate;
            auction.AuctionTime = dto.AuctionTime;
            auction.Status = dto.Status;
            auction.AuctionneerId = dto.AuctionneerId;
            auction.Description = dto.Description;
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
        public AuctionDto SetAuctionTime(Guid id, SetAuctionTimeDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException("Request body is required.");
            }

            var auction = _db.Auctions.Find(id);
            if (auction == null)
            {
                throw new KeyNotFoundException("Auction not found.");
            }

            auction.AuctionTime = dto.AuctionTime;
            _db.SaveChanges();

            return new AuctionDto
            {
                Id = auction.Id,
                Description = auction.Description,
                AuctionDate = auction.AuctionDate,
                AuctionTime = auction.AuctionTime,
                Status = auction.Status,
                Items = new List<AuctionItemDto>()
            };
        }
    }
}

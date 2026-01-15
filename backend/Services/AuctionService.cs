using backend.Db;
using backend.Db.Entities;
using backend.Dtos;
using Microsoft.EntityFrameworkCore;
using System;


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
        public async Task<List<AuctionDto>> GetAllAuctions()
        {
            // Map de Auction entiteiten naar AuctionDto's (zodat we niet per ongeluk te veel info lekken).
            var auctions = await _db.Auctions
                .AsNoTracking()
                .Include(a => a.ClockLocation)
                .ToListAsync();
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
        public async Task<AuctionDto?> GetAuctionById(Guid id)
        {
            var auction = await _db.Auctions
                .AsNoTracking()
                .Include(a => a.ClockLocation)
                .FirstOrDefaultAsync(a => a.Id == id);
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
        public async Task<AuctionDto> CreateAuction(CreateAuctionWithItemsDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException("Request body is required.");
            }

            if (dto.AuctionDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new ArgumentException("Auction date cannot be in the past.");
            }

            if (!dto.ClockLocationId.HasValue)
            {
                throw new ArgumentException("Clock location is required for auctions.");
            }

            var clockLocationExists = await _db.ClockLocations
                .AsNoTracking()
                .AnyAsync(cl => cl.Id == dto.ClockLocationId.Value);
            if (!clockLocationExists)
            {
                throw new ArgumentException($"Clock location {dto.ClockLocationId.Value} does not exist.");
            }

            var productIds = dto.ProductIds ?? new List<Guid>();

            // Batch validate all products at once
            if (productIds.Any())
            {
                var products = await _db.Products
                    .AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                var foundProductIds = products.Select(p => p.Id).ToHashSet();
                var missingProducts = productIds.Where(id => !foundProductIds.Contains(id)).ToList();
                if (missingProducts.Any())
                {
                    throw new ArgumentException($"Products do not exist: {string.Join(", ", missingProducts)}");
                }

                var invalidProducts = products.Where(p => 
                    !p.ClockLocationId.HasValue || 
                    p.ClockLocationId.Value != dto.ClockLocationId.Value).ToList();
                if (invalidProducts.Any())
                {
                    var invalidIds = string.Join(", ", invalidProducts.Select(p => p.Id));
                    throw new ArgumentException($"Products {invalidIds} do not have the correct clock location assigned.");
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
            await _db.SaveChangesAsync();

            foreach (var productId in productIds)
            {

                var auctionItem = new AuctionItem
                {
                    Id = Guid.NewGuid(),
                    AuctionId = auction.Id,
                    ProductId = productId,
                    Status = AuctionItemStatus.Pending
                };

                _db.AuctionItems.Add(auctionItem);
            }

            await _db.SaveChangesAsync();

            // Reload auction with clock location
            var createdAuction = await _db.Auctions
                .AsNoTracking()
                .Include(a => a.ClockLocation)
                .FirstOrDefaultAsync(a => a.Id == auction.Id);

            var items = await _db.AuctionItems
                .AsNoTracking()
                .Where(ai => ai.AuctionId == auction.Id)
                .Select(ai => new AuctionItemDto
                {
                    Id = ai.Id,
                    ProductId = ai.ProductId,
                    Status = ai.Status.ToString()
                })
                .ToListAsync();

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
        public async Task<string> UpdateAuction(Guid id, CreateAuctionWithItemsDto dto, Guid userId, bool isAdmin)
        {
            if (dto == null)
            {
                throw new ArgumentException("Request body is required.");
            }

            var auction = await _db.Auctions.FirstOrDefaultAsync(a => a.Id == id);
            if (auction == null)
            {
                throw new KeyNotFoundException("Auction not found.");
            }

            // Check ownership: auctioneers can only update their own auctions, admins can update any
            if (!isAdmin && auction.AuctionneerId != userId)
            {
                throw new UnauthorizedAccessException("You can only update your own auctions.");
            }

            if (dto.AuctionDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new ArgumentException("Auction date cannot be in the past.");
            }

            if (!dto.ClockLocationId.HasValue)
            {
                throw new ArgumentException("Clock location is required for auctions.");
            }

            var clockLocationExists = await _db.ClockLocations
                .AsNoTracking()
                .AnyAsync(cl => cl.Id == dto.ClockLocationId.Value);
            if (!clockLocationExists)
            {
                throw new ArgumentException($"Clock location {dto.ClockLocationId.Value} does not exist.");
            }

            var productIds = dto.ProductIds ?? new List<Guid>();
            if (productIds.Any())
            {
                // Batch validate all products at once
                var products = await _db.Products
                    .AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                var foundProductIds = products.Select(p => p.Id).ToHashSet();
                var missingProducts = productIds.Where(id => !foundProductIds.Contains(id)).ToList();
                if (missingProducts.Any())
                {
                    throw new ArgumentException($"Products do not exist: {string.Join(", ", missingProducts)}");
                }

                var invalidProducts = products.Where(p => 
                    !p.ClockLocationId.HasValue || 
                    p.ClockLocationId.Value != dto.ClockLocationId.Value).ToList();
                if (invalidProducts.Any())
                {
                    var invalidIds = string.Join(", ", invalidProducts.Select(p => p.Id));
                    throw new ArgumentException($"Products {invalidIds} do not have the correct clock location assigned.");
                }
            }

            auction.AuctionDate = dto.AuctionDate;
            auction.AuctionTime = dto.AuctionTime;
            auction.Status = dto.Status;
            auction.AuctionneerId = dto.AuctionneerId;
            auction.Description = dto.Description;
            auction.ClockLocationId = dto.ClockLocationId;

            await _db.SaveChangesAsync();

            return $"Auction {auction.Id} updated successfully.";
        }

        // Verwijdert een veiling uit de database.
        // Gooit een exceptie als de veiling niet bestaat (het heeft geen zin iets te verwijderen wat er niet is).
        // Anders verwijderen we de veiling en slaan we dat op (digitale equivalent van "weg is weg").
        public async Task<string> DeleteAuction(Guid id, Guid userId, bool isAdmin)
        {
            var auction = await _db.Auctions.FirstOrDefaultAsync(a => a.Id == id);
            if (auction == null)
            {
                throw new KeyNotFoundException("Auction not found.");
            }

            if (!isAdmin && auction.AuctionneerId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own auctions.");
            }

            _db.Auctions.Remove(auction);
            await _db.SaveChangesAsync();

            return $"Auction {auction.Id} deleted successfully.";
        }
        public async Task<AuctionDto> SetAuctionTime(Guid id, SetAuctionTimeDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentException("Request body is required.");
            }

            var auction = await _db.Auctions.FirstOrDefaultAsync(a => a.Id == id);
            if (auction == null)
            {
                throw new KeyNotFoundException("Auction not found.");
            }

            auction.AuctionTime = dto.AuctionTime;
            await _db.SaveChangesAsync();

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

        public async Task<List<AuctionDto>> GetAuctionsWonByBuyer(Guid buyerId)
        {
            var auctionIds = await _db.AuctionItems
                .AsNoTracking()
                .Where(ai => ai.BuyerId == buyerId)
                .Select(ai => ai.AuctionId)
                .Distinct()
                .ToListAsync();

            if (!auctionIds.Any())
            {
                return new List<AuctionDto>();
            }

            var auctions = await _db.Auctions
                .AsNoTracking()
                .Include(a => a.ClockLocation)
                .Where(a => auctionIds.Contains(a.Id))
                .ToListAsync();

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
    }
}

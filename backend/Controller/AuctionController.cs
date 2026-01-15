using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Services;
using backend.Dtos;

namespace backend.Controllers
{
    [Route("auctions")]
    public class AuctionController : Controller
    {
        private readonly AuctionService _auctionService;
        private readonly AuctionLiveService _auctionLiveService;

        public AuctionController(AuctionService auctionService, AuctionLiveService auctionLiveService)
        {
            /*
                Disclaimer: Deze comments zijn puur geplaatst omdat de docent vond dat er
                te weinig comments waren, ook al spreken de namen eigenlijk al voor zich...

                We injecteren hier de AuctionService en AuctionLiveService, zodat de controller 
                zelf lekker dun blijft en alle ingewikkelde logica aan de services overlaat.
            */
            _auctionService = auctionService;
            _auctionLiveService = auctionLiveService;
        }

        // GET: /auctions
        // Haalt gewoon *alle* veilingen op via de service (geen filters of magie).
        // In theorie zou je hier nog pagination of caching kunnen toevoegen, 
        // maar laten we niet te ambitieus doen voor een simpele lijst ophalen.
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllAuctions()
        {
            var auctions = await _auctionService.GetAllAuctions();
            return Ok(auctions);
        }

        // GET: /auctions/won
        [HttpGet("won")]
        [Authorize(Roles = "buyer,admin")]
        public async Task<IActionResult> GetWonAuctions()
        {
            string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid buyerId))
            {
                return Unauthorized("Invalid user id");
            }

            var auctions = await _auctionService.GetAuctionsWonByBuyer(buyerId);
            return Ok(auctions);
        }

        // GET: /auctions/{id}
        // Haalt één specifieke veiling op via de service. 
        // Geeft 404 als de veiling niet bestaat (wie had dat gedacht).
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAuctionById(Guid id)
        {
            var auction = await _auctionService.GetAuctionById(id);
            if (auction == null)
            {
                return NotFound("Auction not found.");
            }
            return Ok(auction);
        }

        // POST: /auctions
        // Maakt een nieuwe veiling aan via de service. 
        // De service valideert de input en slaat de veiling netjes op in de database.
        // Geeft bij succes een 201 Created met het nieuwe veiling-ID en de veilingdetails.
        [HttpPost]
        [Authorize(Roles = "auctioneer,admin")]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionWithItemsDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Request body is required.");
            }

            try
            {
                var createdAuction = await _auctionService.CreateAuction(dto);
                // Return CreatedAtAction met het ID van de nieuwe veiling en de veilinginfo
                return CreatedAtAction(nameof(GetAuctionById), new { id = createdAuction.Id }, createdAuction);
            }
            catch (ArgumentException ex)
            {
                // Ongeldige input (bijv. eindtijd voor starttijd of product bestaat niet)
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, type = ex.GetType().Name });
            }
        }

        // PUT: /auctions/{id}
        // Werkt een bestaande veiling bij via de service.
        // Controleert of de veiling bestaat (we zijn geen tovenaars) en valideert de input.
        [HttpPut("{id}")]
        [Authorize(Roles = "auctioneer,admin")]
        public async Task<IActionResult> UpdateAuction(Guid id, [FromBody] CreateAuctionWithItemsDto dto)
        {
            string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized("Invalid user id");
            }

            var isAdmin = User.IsInRole("admin");

            // Check ownership before calling service
            var existingAuction = await _auctionService.GetAuctionById(id);
            if (existingAuction == null)
            {
                return NotFound("Auction not found.");
            }

            // Get the auction entity to check AuctionneerId
            // We need to check ownership, so we'll pass userId and isAdmin to the service
            try
            {
                var message = await _auctionService.UpdateAuction(id, dto, userId, isAdmin);
                return Ok(message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: /auctions/{id}
        // Verwijdert een veiling via de service.
        // Geeft een foutmelding als de veiling niet bestaat, anders 200 met een succesboodschap.
        [HttpDelete("{id}")]
        [Authorize(Roles = "auctioneer,admin")]
        public async Task<IActionResult> DeleteAuction(Guid id)
        {
            string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized("Invalid user id");
            }

            var isAdmin = User.IsInRole("admin");

            try
            {
                var message = await _auctionService.DeleteAuction(id, userId, isAdmin);
                return Ok(message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: /auctions/{id}/live/start
        // Start een live veiling via de service (als er items zijn).
        // Als de veiling niet bestaat of geen items heeft, krijg je een nette fout terug.
        [HttpPost("{id:guid}/live/start")]
        [Authorize(Roles = "auctioneer,admin")]
        public async Task<IActionResult> StartLive(Guid id)
        {
            try
            {
                var liveAuction = await _auctionLiveService.StartLive(id);
                return Ok(liveAuction);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: /auctions/{id}/live
        // Haalt de huidige live status van de veiling op via de service.
        // Als de live veiling nog niet gestart is, krijg je status "stopped" terug.
        [HttpGet("{id:guid}/live")]
        [Authorize]
        public async Task<IActionResult> GetLive(Guid id)
        {
            var liveStatus = await _auctionLiveService.GetLive(id);
            return Ok(liveStatus);
        }

        // POST: /auctions/{id}/live/bid
        // Plaatst een bod op het huidige item van de live veiling via de service.
        // Controleert eerst of de gebruiker geldig is en laat de service de rest afhandelen.
        [HttpPost("{id:guid}/live/bid")]
        [Authorize(Roles = "buyer,supplier,admin")]
        public async Task<IActionResult> PlaceLiveBid(Guid id, [FromBody] PlaceLiveBidDto dto)
        {
            // Haal het koper-ID uit de claims (dankzij [Authorize] weten we wie er biedt)
            var buyerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(buyerIdString, out Guid buyerId))
            {
                return Unauthorized("Invalid user id");
            }

            try
            {
                var result = await _auctionLiveService.PlaceLiveBid(id, buyerId, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // Bijv. veiling niet aan de gang, geen huidig item, quantity <= 0
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                // Bijv. huidig item niet gevonden (zou niet moeten gebeuren tenzij database gewijzigd)
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Bijv. bod niet op huidige item, al een bod deze ronde, of item al verkocht/voorbij
                return Conflict(ex.Message);
            }
        }

        // POST: /auctions/{id}/live/advance
        // Gaat naar het volgende item in de live veiling via de service.
        // Als de veiling nog niet loopt, kun je natuurlijk niet doorgaan (foutmelding).
        [HttpPost("{id:guid}/live/advance")]
        [Authorize(Roles = "auctioneer,admin")]
        public async Task<IActionResult> AdvanceLive(Guid id)
        {
            try
            {
                var liveStatus = await _auctionLiveService.AdvanceLive(id);
                return Ok(liveStatus);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id:guid}/time")]
        [Authorize(Roles = "auctioneer,admin")]
        public async Task<IActionResult> SetAuctionTime(Guid id, [FromBody] SetAuctionTimeDto dto)
        {
            try
            {
                var updated = await _auctionService.SetAuctionTime(id, dto);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}

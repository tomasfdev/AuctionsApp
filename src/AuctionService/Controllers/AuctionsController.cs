using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

        public AuctionsController(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AuctionDto>>> GetAuctions(string date)
        {
            var query = _context.Auctions.OrderBy(a => a.Item.Make).AsQueryable();  //"AsQueryable" returns IQueryable to be possible to make further queries

            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(a => a.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);    //return auctions that are greater than "date" param
            }

            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuction(Guid id)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction is null) return NotFound();

            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
        {
            var auction = _mapper.Map<Auction>(createAuctionDto);
            //TODO: add current user as seller
            auction.Seller = "test";

            _context.Auctions.Add(auction); //entity framework tracking this in memory, nothing's been saved to the DB

            var result = await _context.SaveChangesAsync() > 0; //save to DB

            if (!result) return BadRequest("Error creating auction");

            return CreatedAtAction(nameof(GetAuction), new { auction.Id }, _mapper.Map<AuctionDto>(auction));   //return an http statusCode201 created Response, where(endpoint) the obj was created, and the obj "auction" created
            //nameof: name of the action/endpoint where the obj("auction") can be found, where can get the obj created. New{auction.Id}: is the parameter that the action needs(Guid id)
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction is null) return NotFound();

            //TODO: check seller == username

            //Mapping entities
            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make; //if null or undefined ?? stay's the same
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.ImageUrl = updateAuctionDto.ImageUrl ?? auction.Item.ImageUrl;

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Error updating auction");

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction is null) return NotFound();

            //TODO: check seller == username

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Error deleting auction");

            return Ok();
        }
    }
}

using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AuctionDto>>> GetAuctions(string date)
        {
            var query = _context.Auctions.OrderBy(a => a.Item.Make).AsQueryable();  //"AsQueryable()" returns IQueryable to be possible to make further queries

            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(a => a.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);    //return auctions that are greater/later than "date" param
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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
        {
            var auction = _mapper.Map<Auction>(createAuctionDto);

            auction.Seller = User.Identity.Name;

            _context.Auctions.Add(auction); //entity framework tracking this in memory, nothing's been saved to the DB

            var newAuction = _mapper.Map<AuctionDto>(auction);  //map Auction to AuctionDto

            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));    //publishes a message as AuctionCreated to RabbitMQ/EventBus for SearchService subscribes/consumes it

            var result = await _context.SaveChangesAsync() > 0; //save to DB

            if (!result) return BadRequest("Error creating auction");

            return CreatedAtAction(nameof(GetAuction), new { auction.Id }, newAuction);   //return an http statusCode201 created Response, where(endpoint) the obj was created, and the obj "auction" created
            //nameof: name of the action/endpoint where the obj("auction") can be found, where can get the obj created. New{auction.Id}: is the parameter that the action needs(Guid id)
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction is null) return NotFound();

            if (auction.Seller != User.Identity.Name) return Forbid();  //check if the person that's updating auction matches the seller

            //Mapping entities
            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make; //if null or undefined ?? stay's the same
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.ImageUrl = updateAuctionDto.ImageUrl ?? auction.Item.ImageUrl;

            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));   //publishes a message as AuctionUpdated to RabbitMQ/EventBus for SearchService subscribes/consumes it

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Error updating auction");

            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction is null) return NotFound();

            if (auction.Seller != User.Identity.Name) return Forbid();  //check if the person that's deleting auction matches the seller;

            _context.Auctions.Remove(auction);

            await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Error deleting auction");

            return Ok();
        }
    }
}

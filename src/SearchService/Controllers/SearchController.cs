using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Item>>> SearchItems([FromQuery] SearchParams searchParams)
        {
            var query = DB.PagedSearch<Item, Item>();    //creates the query, which is an PagedSearch

            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore(); //find something matching "searchTerm"
            }

            query = searchParams.OrderBy switch //Ordering order by
            {
                "make" => query.Sort(x => x.Ascending(i => i.Make)),
                "new" => query.Sort(x => x.Descending(i => i.CreatedAt)),
                _ => query.Sort(x => x.Ascending(i => i.AuctionEnd))
            };

            query = searchParams.FilterBy switch //Filtering filter by
            {
                "finished" => query.Match(i => i.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(i => i.AuctionEnd < DateTime.UtcNow.AddHours(6) && i.AuctionEnd > DateTime.UtcNow), //auctions ending in 6hrs && live auctions
                _ => query.Match(i => i.AuctionEnd > DateTime.UtcNow)   //auctions that are still live
            };

            if (!string.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(i => i.Seller == searchParams.Seller);
            }

            if (!string.IsNullOrEmpty(searchParams.Winner))
            {
                query.Match(i => i.Winner == searchParams.Winner);
            }

            query.PageNumber(searchParams.PageNumber);  //pagination params
            query.PageSize(searchParams.PageSize);

            var result = await query.ExecuteAsync();    //execute query

            return Ok(new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
                //implement pagination using MongoDB entities
            });
        }
    }
}

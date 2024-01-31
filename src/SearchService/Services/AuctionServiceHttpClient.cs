using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Services
{
    public class AuctionServiceHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public AuctionServiceHttpClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<IReadOnlyList<Item>> GetItemsForSearchDb()
        {
            var lastItemUpdated = await DB.Find<Item, string>()   //find type Item return string to get the date and time of the last updated auction/item
                .Sort(x => x.Descending(i => i.UpdatedAt))
                .Project(i => i.UpdatedAt.ToString())   //project to get the string of the date
                .ExecuteFirstAsync();
            //gives the DATE of the auction/item that's been updated, the latest in the db

            //make the call to auction service to send "lastItemUpdated" as the DATE param via a query string
            return await _httpClient.GetFromJsonAsync<IReadOnlyList<Item>>(_config["AuctionServiceUrl"] + "/api/auctions?date=" + lastItemUpdated);
        }
    }
}

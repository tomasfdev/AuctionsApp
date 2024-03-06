using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.Services;

namespace SearchService.Data
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));   //init MongoDB

            await DB.Index<Item>()  //Act like a search server, creates an index for Item entity so that can search for these specific props(Make, Model, Color)
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync();

            var count = await DB.CountAsync<Item>();    //count Items in MongoDB

            //getting data from file
            //if (count == 0)
            //{
            //    Console.WriteLine("No data - will attempt to seed");
            //    var itemData = await File.ReadAllTextAsync("Data/auctions.json");   //read seed file auctions.json

            //    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; //formating opts

            //    var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);  //convert seed file into list<item>

            //    await DB.SaveAsync(items);
            //}

            //using http to get data from AuctionService
            using var scope = app.Services.CreateScope();   //get access to service(AuctionServiceHttpClient service)

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionServiceHttpClient>();  //get service

            var items = await httpClient.GetItemsForSearchDb(); //execute service

            Console.WriteLine(items.Count + " returned from the auction service");

            if (items.Count > 0) await DB.SaveAsync(items); //if get items/auctions save in MongoDb
        }
    }
}
using AuctionService;
using BiddingService.Entities;
using Grpc.Net.Client;

namespace BiddingService.Services
{
    public class GrpcAuctionClient
    {
        private readonly ILogger<GrpcAuctionClient> _logger;
        private readonly IConfiguration _config;

        public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public Auction GetAuction(string id)
        {
            _logger.LogInformation("Calling GRPC Service");
            //create the request
            var channel = GrpcChannel.ForAddress(_config["GrpcAuction"]);   //get Grpc channel
            var client = new GrpcAuction.GrpcAuctionClient(channel);
            var request = new GetAuctionRequest { Id = id };    //the request that goes to Grpc AuctionService, in AuctionService/Services/GrpcAuctionService.cs

            try
            {
                var reply = client.GetAuction(request);
                var auction = new Auction
                {
                    ID = reply.Auction.Id,
                    AuctionEnd = DateTime.Parse(reply.Auction.AuctionEnd),
                    Seller = reply.Auction.Seller,
                    ReservePrice = reply.Auction.ReservePrice
                };

                return auction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not call GRPC Server");
                return null;
            }
        }
    }
}
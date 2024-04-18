using AuctionService.Data;
using Grpc.Core;

namespace AuctionService.Services
{
    public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
    {
        private readonly AuctionDbContext _dbContext;

        public GrpcAuctionService(AuctionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //service that listen Grpc request and send back the Grpc response
        public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
        {
            Console.WriteLine("==> Received Grpc request for auction");

            var auction = await _dbContext.Auctions.FindAsync(Guid.Parse(request.Id));

            if (auction is null) throw new RpcException(new Status(StatusCode.NotFound, "Not Found"));

            var response = new GrpcAuctionResponse
            {
                Auction = new GrpcAuctionModel
                {
                    Id = auction.Id.ToString(),
                    AuctionEnd = auction.AuctionEnd.ToString(),
                    ReservePrice = auction.ReservePrice,
                    Seller = auction.Seller,
                }
            };

            return response;
        }
    }
}
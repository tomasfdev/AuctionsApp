using BiddingService.Entities;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Consumers
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            var auction = new Auction
            {
                ID = context.Message.Id,
                AuctionEnd = context.Message.AuctionEnd,
                Seller = context.Message.Seller,
                ReservePrice = context.Message.ReservePrice
            };

            await auction.SaveAsync();
        }
    }
}

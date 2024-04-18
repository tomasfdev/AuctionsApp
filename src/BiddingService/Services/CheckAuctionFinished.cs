using BiddingService.Entities;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services
{
    public class CheckAuctionFinished : BackgroundService   //implement a long running IHostedService, a singleton service
    {
        private readonly ILogger<CheckAuctionFinished> _logger;
        private readonly IServiceProvider _service;

        public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider service)
        {
            _logger = logger;
            _service = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting check for finished auctions");

            stoppingToken.Register(() => _logger.LogInformation("==> Auction check is stopping"));

            while (!stoppingToken.IsCancellationRequested)  //while app not shut down
            {
                await CheckAuctions(stoppingToken); //check for Auctions that have already finished

                await Task.Delay(5000, stoppingToken); //run task every 5sec
            }
        }

        private async Task CheckAuctions(CancellationToken stoppingToken)
        {
            var finishedAuctions = await DB.Find<Auction>()
                .Match(a => a.AuctionEnd <= DateTime.UtcNow)    //check auctions that date have already ended
                .Match(a => !a.Finished)    //and check auctions that aren't finished
                .ExecuteAsync(stoppingToken);   //if service/stoppingToken stop, this query doesn't continue running

            if (finishedAuctions.Count == 0) return;    //if there isn't "finishedAuctions" stop the execution inside the while loop

            _logger.LogInformation("==> Found {count} auctions that have completed", finishedAuctions.Count);

            using var scope = _service.CreateScope();   //create scope to get access to IPublishEndpoint
            var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();    //get MassTransit service

            foreach (var auction in finishedAuctions)
            {
                auction.Finished = true;
                await auction.SaveAsync(null, stoppingToken);

                var winningBid = await DB.Find<Bid>()
                    .Match(b => b.AuctionId == auction.ID)
                    .Match(b => b.BidStatus == BidStatus.Accepted)
                    .Sort(x => x.Descending(b => b.Amount))
                    .ExecuteFirstAsync(stoppingToken);

                await endpoint.Publish(new AuctionFinished
                {
                    ItemSold = winningBid != null,
                    AuctionId = auction.ID,
                    Winner = winningBid?.Bidder,
                    Seller = auction.Seller,
                    Amount = winningBid?.Amount
                }, stoppingToken);  //publish the event
            }
        }
    }
}

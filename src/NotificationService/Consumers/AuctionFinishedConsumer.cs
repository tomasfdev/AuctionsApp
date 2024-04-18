using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers
{
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public AuctionFinishedConsumer(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        //receive the event and send that event out as a message
        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            Console.WriteLine("--> auction finished message received");

            await _hubContext.Clients.All.SendAsync("AuctionFinished", context.Message); //send the notification/message/context to every connected client
        }
    }
}

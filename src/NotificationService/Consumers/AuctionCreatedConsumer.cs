using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public AuctionCreatedConsumer(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        //receive the event and send that event out as a message
        public async Task Consume(ConsumeContext<AuctionCreated> context)
        {
            Console.WriteLine("--> auction created message received");

            await _hubContext.Clients.All.SendAsync("AuctionCreated", context.Message); //send the notification/message/context to every connected client
        }
    }
}

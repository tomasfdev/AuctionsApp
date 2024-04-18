using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers
{
    public class BidPlacedConsumer : IConsumer<BidPlaced>
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public BidPlacedConsumer(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        //receive the event and send that event out as a message
        public async Task Consume(ConsumeContext<BidPlaced> context)
        {
            Console.WriteLine("--> bid placed message received");

            await _hubContext.Clients.All.SendAsync("BidPlaced", context.Message); //send the notification/message/context to every connected client
        }
    }
}

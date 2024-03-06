using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>> //IConsumer interface from MassTransit
    {
        public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
        {
            Console.WriteLine("--> Consuming faulty creation");

            var exception = context.Message.Exceptions.First(); //get exception

            if (exception.ExceptionType == "System.ArgumentException")  //check if ExceptionType is equal to the ArgumentException created at AuctionCreatedConsumer
            {
                context.Message.Message.Model = "FooBar";   //change Message/AuctionCreated.Model to FooBar
                await context.Publish(context.Message.Message); //and resend Message/AuctionCreated back to EventBus/RabbitMQ
            }
            else
            {
                Console.WriteLine("Not an argument exception - update error dashboard somewhere");
            }
        }
    }
}
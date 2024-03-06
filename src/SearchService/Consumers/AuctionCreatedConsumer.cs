using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreated> //IConsumer interface from MassTransit
    {
        private readonly IMapper _mapper;

        public AuctionCreatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }

        //inside Consume method, what to do when consuming a particular event/message/contract(context=AuctionCreated) when it arrives from the service bus
        public async Task Consume(ConsumeContext<AuctionCreated> context) 
        {
            Console.WriteLine("--> Consuming auction created: " + context.Message.Id);

            var item = _mapper.Map<Item>(context.Message);  //map AuctionCreated(context.Message) to an item to save in MongoDB

            if (item.Model == "Foo") throw new ArgumentException("Cannot sell cars with name of 'Foo'"); 

            await item.SaveAsync(); //save item in MongoDB
        }
    }
}

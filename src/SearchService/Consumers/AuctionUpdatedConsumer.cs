using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers
{
    public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
    {
        private readonly IMapper _mapper;

        public AuctionUpdatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<AuctionUpdated> context)
        {
            Console.WriteLine("--> Consuming auction updated: " + context.Message.Id);

            var item = _mapper.Map<Item>(context.Message);  //map AuctionUpdated(context.Message) to an item to save in MongoDB

            var result = await DB.Update<Item>()
                .MatchID(context.Message.Id)
                //.Match(i => i.ID == context.Message.Id)
                .ModifyOnly(i => new {i.Make, i.Model, i.Year, i.Color, i.Mileage, i.ImageUrl}, item)   //update the props
                .ExecuteAsync();

            if (!result.IsAcknowledged)
                throw new MessageException(typeof(AuctionUpdated), "Problem updating auction");
        }
    }
}

using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);  //map from Auction to AuctionDto including Item
            CreateMap<Item, AuctionDto>();
            CreateMap<CreateAuctionDto, Auction>()
                .ForMember(destinationMember => destinationMember.Item, options => options.MapFrom(sourceMember => sourceMember));    //because have Item props
            CreateMap<CreateAuctionDto, Item>();
        }
    }
}
using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Entities;
using Contracts;

namespace BiddingService.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Bid, BidDto>().ReverseMap();
            CreateMap<Bid, BidPlaced>().ReverseMap();
        }
    }
}

using AutoMapper;

namespace API.Profiles;

public class CityProfile : Profile
{
  public CityProfile()
  {
    CreateMap<Entities.City, Models.CityWithOutPointsOfInterestDto>();
    CreateMap<Entities.City, Models.CityDto>();
  }
}

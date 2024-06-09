using Microsoft.AspNetCore.Mvc;

using API.Interfaces;
using API.Models;
using AutoMapper;

namespace API.Controllers;

// [Route("api/ciudades")]
public class CitiesController : BaseApiController
{
  private readonly ICityInfoRepository _cityInfoRepository;
  private readonly IMapper _mapper;

  public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
  {
    _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
    _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<CityWithOutPointsOfInterestDto>>> GetCities()
  {
    var cities = await _cityInfoRepository.GetCitiesAsync();
    return Ok(_mapper.Map<IEnumerable<CityWithOutPointsOfInterestDto>>(cities));
    // var results = new List<CityWithOutPointsOfInterestDto>();
    // foreach (var city in cities)
    // {
    //   results.Add(new CityWithOutPointsOfInterestDto
    //   {
    //     Id = city.Id,
    //     Name = city.Name,
    //     Description = city.Description,
    //   });
    // }
    // return Ok(results);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetCityById(int id, bool includePointsOfInterest = false)
  {
    var city = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);

    if (city is null)
    {
      return NotFound();
    }

    if (includePointsOfInterest)
    {
      return Ok(_mapper.Map<CityDto>(city));
    }

    return Ok(_mapper.Map<CityWithOutPointsOfInterestDto>(city));
  }

  [HttpGet("Version")]
  public ContentResult GetVersion()
  => Content("v1.0.0");
}

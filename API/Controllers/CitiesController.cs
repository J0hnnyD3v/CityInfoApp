using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// [Route("api/ciudades")]
public class CitiesController : BaseApiController
{
  [HttpGet]
  public ActionResult<IEnumerable<CityDto>> GetCities()
  {
    return Ok(CitiesDataStore.Current.Cities);
  }

  [HttpGet("{id}")]
  public ActionResult<CityDto> GetCityById(int id)
  {
    // find city
    var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id);

    if (cityToReturn is null)
    {
      return NotFound($"City with id: {id} was not found");
    }

    return Ok(cityToReturn);
  }

  [HttpGet("Version")]
  public ContentResult GetVersion()
    => Content("v1.0.0");
}

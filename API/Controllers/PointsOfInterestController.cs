using Microsoft.AspNetCore.Mvc;

using API.Controllers;
using API.Models;

namespace API;

[Route("api/cities/{cityId}/[controller]")]
public class PointsOfInterestController : BaseApiController
{
  [HttpGet]
  public ActionResult<IEnumerable<PointOfInterestDto>> GetPointOfInterest(int cityId)
  {
    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

    if (city is null)
    {
      return NotFound($"City with id: {cityId} was not found.");
    }

    return Ok(city.PointsOfInterest);
  }

  [HttpGet("{pointOfInterestId}")]
  public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
  {
    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

    if (city is null)
    {
      return NotFound($"City with id: {cityId} was not found.");
    }

    var pointOfInterest = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);

    if (pointOfInterest is null)
    {
      return NotFound($"Point of interest with id: {pointOfInterestId} was not found.");
    }

    return Ok(pointOfInterest);
  }
}

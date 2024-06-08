using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

using API.Controllers;
using API.Models;
using API.Interfaces;

namespace API;

[Route("api/cities/{cityId}/[controller]")]
public class PointsOfInterestController : BaseApiController
{
  private readonly ILogger<PointsOfInterestController> _logger;
  private readonly IMailService _mailService;
  private readonly CitiesDataStore _citiesDataStore;

  public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, CitiesDataStore citiesDataStore)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
    _citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
  }

  [HttpGet]
  public ActionResult<IEnumerable<PointOfInterestDto>> GetPointOfInterest(int cityId)
  {
    try
    {
      var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

      if (city is null)
      {
        _logger.LogInformation($"City with id: {cityId} was not found when accessing points of interest.");
        return NotFound();
      }

      return Ok(city.PointsOfInterest);
    }
    catch (Exception ex)
    {
      _logger.LogCritical($"Exception while getting point of interest for city with id {cityId}", ex);
      return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error, a problem happened while handling your request");
    }
  }

  [HttpGet("{pointOfInterestId}")]
  public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
  {
    var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

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

  [HttpPost]
  public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
  {
    var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

    if (city is null)
    {
      return NotFound($"City with id: {cityId} was not found.");
    }

    /* demo purposes - to be improved */
    var maxPointOfInterestId = _citiesDataStore.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

    var finalPointOfInterest = new PointOfInterestDto
    {
      Id = maxPointOfInterestId + 1,
      Name = pointOfInterest.Name,
      Description = pointOfInterest.Description
    };

    city.PointsOfInterest.Add(finalPointOfInterest);

    // return CreatedAtRoute
    return CreatedAtAction(
      nameof(GetPointOfInterest),
      new
      {
        cityId,
        pointOfInterestId = finalPointOfInterest.Id
      },
      finalPointOfInterest
    );
  }

  [HttpPut("{pointOfInterestId}")]
  public ActionResult UpdatePointOfInterest(int cityId, int pointOfInterestId, [FromBody] PointOfInterestForUpdateDto pointOfInterest)
  {
    var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

    if (city is null)
    {
      return NotFound($"City with id: {cityId} was not found.");
    }

    var pointOfInterestToUpdate = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);

    if (pointOfInterestToUpdate is null)
    {
      return NotFound($"Point of interest with id: {pointOfInterestId} was not found.");
    }

    pointOfInterestToUpdate.Name = pointOfInterest.Name;
    pointOfInterestToUpdate.Description = pointOfInterest.Description;

    return NoContent();
  }

  [HttpPatch("{pointOfInterestId}")]
  public ActionResult PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
  {
    var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

    if (city is null)
    {
      return NotFound();
    }

    var pointOfInterestToUpdate = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);

    if (pointOfInterestToUpdate is null)
    {
      return NotFound($"Point of interest with id: {pointOfInterestId} was not found.");
    }

    var pointOfInterestToPatch = new PointOfInterestForUpdateDto
    {
      Name = pointOfInterestToUpdate.Name,
      Description = pointOfInterestToUpdate.Description
    };

    patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

    if (!ModelState.IsValid)
    {
      return ValidationProblem(ModelState);
    }

    if (!TryValidateModel(pointOfInterestToPatch))
    {
      return ValidationProblem(ModelState);
    }

    pointOfInterestToUpdate.Name = pointOfInterestToPatch.Name;
    pointOfInterestToUpdate.Description = pointOfInterestToPatch.Description;

    return NoContent();
  }

  [HttpDelete("{pointOfInterestId}")]
  public ActionResult DeletePointOfInterest(int cityId, int pointOfInterestId)
  {
    var city = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

    if (city is null)
    {
      return NotFound($"City with id: {cityId} was not found.");
    }

    var pointOfInterestToDelete = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointOfInterestId);

    if (pointOfInterestToDelete is null)
    {
      return NotFound($"Point of interest with id: {pointOfInterestId} was not found.");
    }

    city.PointsOfInterest.Remove(pointOfInterestToDelete);

    _mailService.Send("Point of interest deleted", $"Point of interest {pointOfInterestToDelete.Name} with id {pointOfInterestId} was deleted.");

    return NoContent();
  }
}

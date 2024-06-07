using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

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

  [HttpPost]
  public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
  {
    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

    if (city is null)
    {
      return NotFound($"City with id: {cityId} was not found.");
    }

    /* demo purposes - to be improved */
    var maxPointOfInterestId = CitiesDataStore.Current.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

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
    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

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
    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

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
    var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

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

    return NoContent();
  }
}

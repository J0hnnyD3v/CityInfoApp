using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

using API.Controllers;
using API.Models;
using API.Interfaces;
using AutoMapper;

namespace API;

[Route("api/cities/{cityId}/[controller]")]
public class PointsOfInterestController : BaseApiController
{
  private readonly ILogger<PointsOfInterestController> _logger;
  private readonly IMailService _mailService;
  private readonly ICityInfoRepository _cityInfoRepository;
  private readonly IMapper _mapper;

  public PointsOfInterestController(
    ILogger<PointsOfInterestController> logger,
    IMailService mailService,
    ICityInfoRepository cityInfoRepository,
    IMapper mapper
  )
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
    _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
    _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
  {
    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      _logger.LogInformation($"City with id: {cityId} was not found when accessing point of interest.");
      return NotFound();
    }

    var pointsOFinterest = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);
    return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOFinterest));
  }

  [HttpGet("{pointOfInterestId}", Name = "GetPointOfInterest")]
  public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointOfInterestId)
  {
    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      _logger.LogInformation($"City with id: {cityId} was not found when accessing city.");
      return NotFound();
    }

    var pointOfInterest = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

    if (pointOfInterest is null)
    {
      _logger.LogInformation($"Point of interest with id: {pointOfInterestId} was not found when accessing point of interest.");
      return NotFound();
    }

    return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));
  }

  [HttpPost]
  public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
  {
    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      _logger.LogInformation($"City with id: {cityId} was not found.");
      return NotFound();
    }

    var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);

    await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);
    await _cityInfoRepository.SaveChangesAsync();

    var createdPointOfInterestToReturn = _mapper.Map<PointOfInterestDto>(finalPointOfInterest);

    return CreatedAtRoute("GetPointOfInterest",
      new
      {
        cityId,
        pointOfInterestId = createdPointOfInterestToReturn.Id
      },
      createdPointOfInterestToReturn);
  }

  [HttpPut("{pointOfInterestId}")]
  public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId, [FromBody] PointOfInterestForUpdateDto pointOfInterest)
  {
    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      _logger.LogInformation($"City with id: {cityId} was not found.");
      return NotFound();
    }

    var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

    if (pointOfInterestEntity is null)
    {
      _logger.LogInformation($"Point of interest with id: {pointOfInterestId} was not found.");
      return NotFound();
    }

    _mapper.Map(pointOfInterest, pointOfInterestEntity);

    await _cityInfoRepository.SaveChangesAsync();

    return NoContent();
  }

  [HttpPatch("{pointOfInterestId}")]
  public async Task<ActionResult> PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
  {
    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      _logger.LogInformation($"City with id: {cityId} was not found.");
      return NotFound();
    }

    var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

    if (pointOfInterestEntity is null)
    {
      _logger.LogInformation($"Point of interest with id: {pointOfInterestId} was not found.");
      return NotFound();
    }

    var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

    patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

    if (!ModelState.IsValid)
    {
      return ValidationProblem(ModelState);
    }

    if (!TryValidateModel(pointOfInterestToPatch))
    {
      return ValidationProblem(ModelState);
    }

    _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

    await _cityInfoRepository.SaveChangesAsync();

    return NoContent();
  }

  [HttpDelete("{pointOfInterestId}")]
  public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
  {
    if (!await _cityInfoRepository.CityExistsAsync(cityId))
    {
      _logger.LogInformation($"City with id: {cityId} was not found.");
      return NotFound();
    }

    var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

    if (pointOfInterestEntity is null)
    {
      _logger.LogInformation($"Point of interest with id: {pointOfInterestId} was not found.");
      return NotFound();
    }

    _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
    await _cityInfoRepository.SaveChangesAsync();

    _mailService.Send("Point of interest deleted", $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");

    return NoContent();
  }
}

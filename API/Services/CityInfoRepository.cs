using Microsoft.EntityFrameworkCore;

using API.Entities;
using API.Interfaces;

namespace API.Services;

public class CityInfoRepository : ICityInfoRepository
{
  private readonly CityInfoContext _cityInfoContext;

  public CityInfoRepository(CityInfoContext cityInfoContext)
  {
    _cityInfoContext = cityInfoContext ?? throw new ArgumentNullException(nameof(cityInfoContext));
  }

  public async Task<IEnumerable<City>> GetCitiesAsync()
  {
    return await _cityInfoContext.Cities.OrderBy(city => city.Name).ToListAsync();
  }

  public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize)
  {
    // collection to start from
    var collection = _cityInfoContext.Cities as IQueryable<City>;

    if (!string.IsNullOrEmpty(name))
    {
      name = name.Trim();
      collection = collection.Where(city => city.Name == name);
    }

    if (!string.IsNullOrWhiteSpace(searchQuery))
    {
      searchQuery = searchQuery.Trim();
      collection = collection.Where(city => city.Name.Contains(searchQuery)
          || (city.Description != null && city.Description.Contains(searchQuery)));
    }

    var totalItemCount = await collection.CountAsync();

    var paginationMetadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber);

    var collectionToReturn = await collection
                                      .Skip(pageSize * (pageNumber - 1))
                                      .Take(pageSize)
                                      .OrderBy(city => city.Name).ToListAsync();

    return (collectionToReturn, paginationMetadata);
  }

  public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
  {
    if (includePointsOfInterest)
    {
      // return await _cityInfoContext.Cities.Include(city => city.PointsOfInterest).FirstOrDefaultAsync(city => city.Id == cityId);
      return await _cityInfoContext.Cities
        .Include(city => city.PointsOfInterest)
        .Where(city => city.Id == cityId)
        .FirstOrDefaultAsync();
    }
    // return await _cityInfoContext.Cities.FirstOrDefaultAsync(city => city.Id == cityId);
    var cities = await _cityInfoContext.Cities
      .Where(city => city.Id == cityId)
      .FirstOrDefaultAsync();

    return cities;
  }
  public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
  {
    return await _cityInfoContext.PointsOfInterest
      .Where(point => point.CityId == cityId)
      .ToListAsync();
  }

  public async Task<bool> CityExistsAsync(int cityId)
  {
    return await _cityInfoContext.Cities.AnyAsync(city => city.Id == cityId);
  }

  public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId)
  {
    return await _cityInfoContext.PointsOfInterest
      .Where(point => point.CityId == cityId && point.Id == pointOfInterestId)
      .FirstOrDefaultAsync();
  }

  public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
  {
    var city = await GetCityAsync(cityId, false);
    // if (city != null)
    // {
    //   city.PointsOfInterest.Add(pointOfInterest);
    // }
    city?.PointsOfInterest.Add(pointOfInterest);
  }

  public async Task<bool> SaveChangesAsync()
  {
    return await _cityInfoContext.SaveChangesAsync() >= 0;
  }

  public void DeletePointOfInterest(PointOfInterest pointOfInterest)
  {
    _cityInfoContext.PointsOfInterest.Remove(pointOfInterest);
  }
}

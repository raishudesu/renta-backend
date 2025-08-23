using backend.Common.Pagination;
using backend.Data;
using backend.DTOs.VehicleDto;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static backend.Helpers.Haversine;

namespace backend.Services;

public class VehicleService(AppDbContext context)
{
    private readonly AppDbContext db = context;

    public async Task<Vehicle> CreateVehicle(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        vehicle.Owner = await db.Users.FindAsync(vehicle.OwnerId) ?? throw new ArgumentException("Invalid OwnerId.", nameof(vehicle.OwnerId));

        db.Vehicle.Add(vehicle);
        await db.SaveChangesAsync();

        return vehicle;
    }

    public async Task<PagedList<Vehicle>> GetVehicles(VehicleParameters vehicleParameters)
    {
        ArgumentNullException.ThrowIfNull(vehicleParameters);

        var vehiclesQuery = db.Vehicle
            .Include(v => v.Owner)
            .AsQueryable();

        if (vehicleParameters.Type.HasValue)
        {
            vehiclesQuery = vehiclesQuery.Where(v => v.Type == vehicleParameters.Type);
        }

        if (!string.IsNullOrWhiteSpace(vehicleParameters.ModelName))
        {
            vehiclesQuery = vehiclesQuery.Where(v => EF.Functions.ILike(v.ModelName, $"%{vehicleParameters.ModelName}%"));
        }

        // ✅ Distance filter if user provided location
        if (vehicleParameters.Latitude.HasValue && vehicleParameters.Longitude.HasValue)
        {
            var userLat = vehicleParameters.Latitude.Value;
            var userLng = vehicleParameters.Longitude.Value;

            var filteredList = await vehiclesQuery
                .Where(v => !string.IsNullOrEmpty(v.Owner.BusinessCoordinatesString))
                .ToListAsync();

            var filtered = filteredList
                .Select(v =>
                {
                    var coords = JsonConvert.DeserializeObject<Coordinates>(v.Owner.BusinessCoordinatesString);
                    if (coords == null)
                        return null;
                    var distance = UseHaversine(userLat, userLng, coords.Lat, coords.Lng);
                    return new { Vehicle = v, Distance = distance };
                })
                .Where(x => x != null);

            if (vehicleParameters.MaxDistanceKm.HasValue)
                filtered = filtered.Where(x => x!.Distance <= vehicleParameters.MaxDistanceKm.Value);

            // order by nearest and paginate in memory
            var orderedVehicles = filtered
                .OrderBy(x => x!.Distance)
                .Select(x => x!.Vehicle)
                .ToList();

            var pagedVehicles = orderedVehicles
                .Skip((vehicleParameters.PageNumber - 1) * vehicleParameters.PageSize)
                .Take(vehicleParameters.PageSize)
                .ToList();

            var filteredCount = orderedVehicles.Count();

            return new PagedList<Vehicle>(pagedVehicles, filteredCount, vehicleParameters.PageNumber, vehicleParameters.PageSize);
        }

        var vehicles = await vehiclesQuery
            .Skip((vehicleParameters.PageNumber - 1) * vehicleParameters.PageSize)
            .Take(vehicleParameters.PageSize)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync();

        var count = await vehiclesQuery.CountAsync();

        return new PagedList<Vehicle>(vehicles, count, vehicleParameters.PageNumber, vehicleParameters.PageSize);
    }

    // PAGINATE
    public async Task<List<Vehicle>> GetVehiclesByUserId(string userId)
    {
        var vehicles = await db.Vehicle.Where(v => v.OwnerId == userId).ToListAsync();

        return vehicles;
    }

    public async Task<Vehicle?> GetVehicleById(Guid id)
    {
        var vehicle = await db.Vehicle.FindAsync(id);

        return vehicle;
    }

    // CREATE A DTO TO PREVENT OVERWRITING RISKS
    public async Task UpdateVehicle(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        db.Vehicle.Update(vehicle);

        await db.SaveChangesAsync();
    }

    public async Task DeleteVehicleById(Guid id)
    {
        var vehicle = await db.Vehicle.FindAsync(id)
        ?? throw new KeyNotFoundException($"Vehicle with ID: {id} not found");

        db.Vehicle.Remove(vehicle);

        await db.SaveChangesAsync();


    }

    public async Task<int> GetTotalVehiclesByUserId(string userId)
    {
        return await db.Vehicle.CountAsync(v => v.OwnerId == userId);
    }


}
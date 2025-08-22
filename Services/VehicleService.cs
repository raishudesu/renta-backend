using backend.Common.Pagination;
using backend.Data;
using backend.DTOs.VehicleDto;
using backend.Models;
using Microsoft.EntityFrameworkCore;

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

        var vehicles = await vehiclesQuery
            .Skip((vehicleParameters.PageNumber - 1) * vehicleParameters.PageSize)
            .Take(vehicleParameters.PageSize)
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
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class VehicleService(AppDbContext context)
{
    private readonly AppDbContext db = context;

    public async Task<Vehicle> CreateVehicle(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        db.Vehicle.Add(vehicle);
        await db.SaveChangesAsync();

        return vehicle;
    }

    public async Task<List<Vehicle>> GetVehicles()
    {
        var vehicles = await db.Vehicle.ToListAsync();

        return vehicles;
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
}
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class VehicleImageService(AppDbContext context)
{
    private readonly AppDbContext db = context;

    public async Task<VehicleImage> CreateVehicleImage(VehicleImage vehicleImage)
    {
        ArgumentNullException.ThrowIfNull(vehicleImage);

        db.VehicleImage.Add(vehicleImage);
        await db.SaveChangesAsync();

        return vehicleImage;
    }
    public async Task<VehicleImage?> GetVehicleImageById(Guid id)
    {
        var vehicleImage = await db.VehicleImage.FindAsync(id);

        return vehicleImage;
    }

    public async Task<List<VehicleImage>> GetVehicleImagesByVehicleId(Guid vehicleId)
    {
        var vehicleImages = await db.VehicleImage.Where(v => v.VehicleId == vehicleId).ToListAsync();

        return vehicleImages;
    }

    public async Task DeleteVehicleImageById(Guid id)
    {
        var vehicleImage = await db.VehicleImage.FindAsync(id)
            ?? throw new KeyNotFoundException($"Vehicle image with ID: {id} not found");

        db.VehicleImage.Remove(vehicleImage);
        await db.SaveChangesAsync();
    }
}
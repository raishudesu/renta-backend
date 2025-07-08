using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class MediumTypeService(AppDbContext context)
{
    private readonly AppDbContext db = context;

    public async Task<MediumType> CreateMediumType(MediumType mediumType)
    {
        ArgumentNullException.ThrowIfNull(mediumType);

        db.MediumType.Add(mediumType);
        await db.SaveChangesAsync();

        return mediumType;
    }

    // NO NEED TO PAGINATE
    public async Task<List<MediumType>> GetMediumTypes()
    {
        var mediumTypes = await db.MediumType.ToListAsync();

        return mediumTypes;
    }

    public async Task<MediumType?> GetMediumTypeById(int id)
    {
        var mediumType = await db.MediumType.FindAsync(id);

        return mediumType;
    }

    public async Task UpdateMediumType(int id, MediumType newMediumType)
    {
        var oldMediumType = await db.MediumType.FindAsync(id) ?? throw new KeyNotFoundException($"Medium Type with ID: {id} not found");

        ArgumentNullException.ThrowIfNull(newMediumType);

        db.MediumType.Update(newMediumType);

        await db.SaveChangesAsync();
    }

    public async Task DeleteMediumTypeById(int id)
    {
        var mediumType = await db.MediumType.FindAsync(id) ?? throw new KeyNotFoundException($"Medium Type with ID: {id} not found");

        db.MediumType.Remove(mediumType);

        await db.SaveChangesAsync();

    }
}
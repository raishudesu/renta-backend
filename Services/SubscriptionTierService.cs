using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class SubscriptionTierService(AppDbContext context)
    {
        private readonly AppDbContext db = context;

        public async Task<SubscriptionTier> CreateSubscriptionTier(SubscriptionTier subTier)
        {
            db.SubscriptionTier.Add(subTier);

            await db.SaveChangesAsync();

            return subTier;
        }

        public async Task<List<SubscriptionTier>> GetSubscriptionTiers()
        {
            var subTiers = await db.SubscriptionTier.ToListAsync();

            return subTiers;
        }

        public async Task<SubscriptionTier?> GetSubscriptionTierById(int id)
        {
            var subTier = await db.SubscriptionTier.FindAsync(id);

            return subTier;
        }
    }
}
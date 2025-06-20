using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class SubscriptionService(AppDbContext context)
    {
        private readonly AppDbContext db = context;

        public async Task<Subscription> CreateSubscription(Subscription subscription)
        {
            ArgumentNullException.ThrowIfNull(subscription);

            db.Subscription.Add(subscription);

            await db.SaveChangesAsync();

            return subscription;
        }

        // PAGINATE
        public async Task<List<Subscription>> GetSubscriptions()
        {
            var subs = await db.Subscription.Include(s => s.PaymentDetails).ToListAsync();

            return subs;
        }

        public async Task<List<Subscription>> GetSubscriptionsByUserId(string id)
        {
            var userSubs = await db.Subscription.Where(s => s.UserId == id).Include(s => s.PaymentDetails).Include(s => s.SubscriptionTier).ToListAsync();

            return userSubs;
        }


        public async Task<Subscription?> GetSubscriptionById(Guid id)
        {
            var sub = await db.Subscription.FindAsync(id);

            return sub;
        }

        public async Task<Subscription?> GetLatestSubscriptionOfUser(string id)
        {
            var latestSub = await db.Subscription.Where(s => s.UserId == id).Include(s => s.PaymentDetails).OrderByDescending(s => s.StartTime).FirstOrDefaultAsync();

            return latestSub;
        }

    }
}
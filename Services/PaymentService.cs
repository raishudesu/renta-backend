

using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class PaymentService(AppDbContext context)
    {

        private readonly AppDbContext db = context;

        public async Task<Payment> CreatePayment(Payment payment)
        {
            ArgumentNullException.ThrowIfNull(payment);

            db.Payment.Add(payment);
            await db.SaveChangesAsync();

            return payment;
        }

        public async Task<List<Payment>> GetPayments()
        {
            var payments = await db.Payment.ToListAsync();

            return payments;
        }

    }
}
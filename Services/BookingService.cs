using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class BookingService
{
    private readonly AppDbContext db;

    public BookingService(AppDbContext context)
    {
        db = context;
    }

    public async Task<List<Booking>> GetBookings()
    {
        try
        {
            var bookings = await db.Bookings.ToListAsync();

            return bookings;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching bookings from database", ex);
        }
    }

    public async Task<Booking?> GetBookingById(Guid id)
    {
        try
        {
            var booking = await db.Bookings.Include(b => b.Vehicle).FirstOrDefaultAsync(b => b.Id == id);

            return booking;
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching booking from database", ex);
        }
    }

    public async Task<List<Booking>> GetBookingsByUserId(string id)
    {
        try
        {
            var bookings = await db.Bookings.Where(b => b.UserId == id).ToListAsync();

            return bookings;
        }
        catch (Exception ex)
        {

            throw new Exception($"Error fetching bookings with ID {id}", ex);
        }
    }

    // caution
    // this could overwrite fields to null if a field is not provided
    public async Task UpdateBooking(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        try
        {
            db.Bookings.Update(booking);

            await db.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            throw new Exception("Failed to update booking", ex);
        }
    }

    public async Task UpdateBookingStatusById(Guid id, BookingStatus newStatus)
    {

        try
        {
            var booking = await db.Bookings.FindAsync(id)
                ?? throw new Exception($"Booking with ID: {id} not found");

            booking.Status = newStatus;

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {

            throw new Exception("Failed to update booking status", ex);

        }

    }

    public async Task DeleteBookingById(Guid id)
    {

        try
        {
            var booking = await db.Bookings.FindAsync(id)
            ?? throw new Exception($"Booking with ID: {id} not found");

            db.Bookings.Remove(booking);

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {

            throw new Exception("Failed to delete booking", ex);

        }

    }
}
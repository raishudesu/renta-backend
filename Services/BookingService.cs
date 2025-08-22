using backend.Common.Pagination;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class BookingService(AppDbContext context)
{
    private readonly AppDbContext db = context;


    public async Task<Booking> CreateBooking(Booking booking)
    {

        ArgumentNullException.ThrowIfNull(booking);

        db.Booking.Add(booking);
        await db.SaveChangesAsync();

        return booking;

    }

    public async Task<PagedList<Booking>> GetBookings(PaginationParameters bookingParameters)
    {

        ArgumentNullException.ThrowIfNull(bookingParameters);

        var bookingsQuery = db.Booking
            .Include(b => b.Vehicle);

        var bookings = await bookingsQuery
            .Skip((bookingParameters.PageNumber - 1) * bookingParameters.PageSize)
            .Take(bookingParameters.PageSize)
            .ToListAsync();

        var count = await bookingsQuery.CountAsync();

        return new PagedList<Booking>(bookings, count, bookingParameters.PageNumber, bookingParameters.PageSize);

    }

    public async Task<Booking?> GetBookingById(Guid id)
    {

        var booking = await db.Booking.Include(b => b.Vehicle).FirstOrDefaultAsync(b => b.Id == id);

        return booking;

    }

    public async Task<PagedList<Booking>> GetBookingsByUserId(string id, PaginationParameters bookingParameters)
    {

        ArgumentNullException.ThrowIfNull(bookingParameters);

        var bookingsQuery = db.Booking
            .Where(b => b.UserId == id)
            .Include(b => b.Vehicle);



        var bookings = await bookingsQuery
            .Skip((bookingParameters.PageNumber - 1) * bookingParameters.PageSize)
            .Take(bookingParameters.PageSize)
            .ToListAsync();

        var count = await bookingsQuery.CountAsync();

        return new PagedList<Booking>(bookings, count, bookingParameters.PageNumber, bookingParameters.PageSize);

    }

    // caution
    // this could overwrite fields to null if a field is not provided
    public async Task UpdateBooking(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        db.Booking.Update(booking);

        await db.SaveChangesAsync();

    }

    public async Task UpdateBookingStatusById(Guid id, BookingStatus newStatus)
    {

        var booking = await db.Booking.FindAsync(id)
            ?? throw new KeyNotFoundException($"Booking with ID: {id} not found");

        booking.Status = newStatus;

        await db.SaveChangesAsync();

    }

    public async Task DeleteBookingById(Guid id)
    {

        var booking = await db.Booking.FindAsync(id)
        ?? throw new KeyNotFoundException($"Booking with ID: {id} not found");

        db.Booking.Remove(booking);

        await db.SaveChangesAsync();

    }

    public async Task<int> GetTotalBookingsByUserId(string userId)
    {
        return await db.Booking.CountAsync(b => b.UserId == userId);
    }
}
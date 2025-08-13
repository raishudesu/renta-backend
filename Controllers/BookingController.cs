

using backend.DTOs.BookingDto;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BookingController(BookingService bookingService) : ControllerBase
{
    private readonly BookingService _bookingService = bookingService;

    [HttpGet]
    [Authorize(Roles = nameof(RoleTypes.Admin))]
    public async Task<ActionResult<List<Booking>>> GetAll()
    {
        var bookings = await _bookingService.GetBookings();

        return Ok(bookings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Booking>> GetById(Guid id)
    {
        var booking = await _bookingService.GetBookingById(id);

        return booking != null ? Ok(booking) : NotFound();
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<ActionResult<List<Booking>>> GetByUserId(string userId)
    {
        var bookings = await _bookingService.GetBookingsByUserId(userId);

        return Ok(bookings);
    }

    [HttpPost]
    // [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<ActionResult<Booking>> Create([FromBody] BookingDto booking)
    {
        var bookingData = new Booking
        {
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            BookerName = booking.BookerName,
            BookerEmail = booking.BookerEmail,
            BookerPhone = booking.BookerPhone,
            UserId = booking.UserId,
            VehicleId = booking.VehicleId
        };

        var createdBooking = await _bookingService.CreateBooking(bookingData);

        return CreatedAtAction(nameof(GetById), new { id = createdBooking.Id }, createdBooking);
    }

    [HttpPut("{id}")]
    // [Authorize(Roles = nameof(RoleTypes.User))]

    public async Task<IActionResult> Update(Guid id, Booking booking)
    {
        if (id != booking.Id) return BadRequest();

        await _bookingService.UpdateBooking(booking);

        return NoContent();
    }

    [HttpDelete("{id}")]
    // [Authorize(Roles = nameof(RoleTypes.User))]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _bookingService.DeleteBookingById(id);

        return NoContent();

    }
}
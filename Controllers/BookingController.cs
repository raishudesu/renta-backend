

using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class BookingController(BookingService bookingService) : ControllerBase
{
    private readonly BookingService _bookingService = bookingService;

    [HttpGet]
    // [Authorize]
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

    [HttpPost]
    public async Task<ActionResult<Booking>> Create(Booking booking)
    {
        var createdBooking = await _bookingService.CreateBooking(booking);

        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Booking booking)
    {
        if (id != booking.Id) return BadRequest();

        await _bookingService.UpdateBooking(booking);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _bookingService.DeleteBookingById(id);

        return NoContent();

    }
}
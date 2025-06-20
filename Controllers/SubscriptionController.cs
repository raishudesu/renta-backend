

using backend.DTOs.Subscription;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]

public class SubscriptionController(SubscriptionService subscriptionService, PaymentService paymentService, SubscriptionTierService subscriptionTierService) : ControllerBase
{
    private readonly SubscriptionService _subscriptionService = subscriptionService;

    private readonly SubscriptionTierService _subscriptionTierService = subscriptionTierService;

    private readonly PaymentService _paymentService = paymentService;

    [HttpGet]
    public async Task<ActionResult<List<Subscription>>> GetAll()
    {
        var subs = await _subscriptionService.GetSubscriptions();

        return Ok(subs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Subscription>> GetById(Guid id)
    {
        var sub = await _subscriptionService.GetSubscriptionById(id);

        return sub != null ? Ok(sub) : NotFound();
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<Subscription>>> GetByUserId(string userId)
    {
        var userSubs = await _subscriptionService.GetSubscriptionsByUserId(userId);

        return Ok(userSubs);
    }

    [HttpPost]
    public async Task<ActionResult<Subscription>> Create([FromBody] CreateSubscriptionWithPaymentDto dto)
    {
        var tier = await _subscriptionTierService.GetSubscriptionTierById(dto.SubscriptionTierId)
        ?? throw new KeyNotFoundException("Subscription tier not found.");

        var payment = new Payment
        {
            // Id = Guid.NewGuid(),
            MediumType = dto.Payment.MediumType,
            ProviderName = dto.Payment.ProviderName,
            Amount = tier.Price,
            ReceiptImageLink = dto.Payment.ReceiptImageLink,
            TransactionId = dto.Payment.TransactionId
        };

        var createdPayment = await _paymentService.CreatePayment(payment);

        var sub = new Subscription
        {
            // Id = Guid.NewGuid(),
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            UserId = dto.UserId,
            SubscriptionTierId = tier.Id,
            PaymentId = payment.Id   // 🗝️ link new payment
        };

        var createdSub = await _subscriptionService.CreateSubscription(sub);

        return CreatedAtAction(nameof(GetById), new { id = sub.Id }, sub);
    }

    [HttpGet("user/{userId}/latest")]
    public async Task<ActionResult<Subscription>> GetLatestSubscriptionOfUser(string userId)
    {
        var latest = await _subscriptionService.GetLatestSubscriptionOfUser(userId);

        if (latest == null)
            return NotFound($"No subscription found for user {userId}.");

        return Ok(latest);
    }
}
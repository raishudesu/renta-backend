using backend.DTOs.Subscription;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;


[ApiController]
[Route("api/[controller]")]

public class SubscriptionTierController(SubscriptionTierService subscriptionTierService) : ControllerBase
{

    private readonly SubscriptionTierService _subscriptionTierService = subscriptionTierService;

    [HttpGet]
    public async Task<ActionResult<SubscriptionTier>> GetById(int id)
    {
        var subTier = await _subscriptionTierService.GetSubscriptionTierById(id);

        return subTier != null ? Ok(subTier) : NotFound();
    }


    [HttpPost]
    public async Task<ActionResult<SubscriptionTier>> Create(SubscriptionTier subscriptionTier)
    {
        var createdTier = await _subscriptionTierService.CreateSubscriptionTier(subscriptionTier);

        return CreatedAtAction(nameof(GetById), new { id = subscriptionTier.Id }, subscriptionTier);
    }
}
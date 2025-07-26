

using backend.DTOs.MediumType;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]

public class MediumTypeController(MediumTypeService mediumTypeService) : ControllerBase
{
    private readonly MediumTypeService _mediumTypeService = mediumTypeService;

    [HttpGet]
    [Authorize(Roles = nameof(RoleTypes.Admin))]

    public async Task<ActionResult<List<MediumType>>> GetAll()
    {
        var mediumTypes = await _mediumTypeService.GetMediumTypes();

        return Ok(mediumTypes);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = nameof(RoleTypes.Admin))]

    public async Task<ActionResult<MediumType>> GetById(int id)
    {
        var mediumType = await _mediumTypeService.GetMediumTypeById(id);

        return mediumType != null ? Ok(mediumType) : NotFound();
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleTypes.Admin))]

    public async Task<ActionResult<MediumType>> Create(MediumType mediumType)
    {
        var createdType = await _mediumTypeService.CreateMediumType(mediumType);

        return CreatedAtAction(nameof(GetById), new { id = mediumType.Id }, mediumType);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(RoleTypes.Admin))]

    public async Task<IActionResult> Update(int id, [FromBody] MediumTypeDto mediumType)
    {
        try
        {
            var existingMediumType = await _mediumTypeService.GetMediumTypeById(id);
            if (existingMediumType == null)
            {
                return NotFound();
            }

            existingMediumType.Name = mediumType.Name;

            await _mediumTypeService.UpdateMediumType(id, existingMediumType);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(RoleTypes.Admin))]

    public async Task<IActionResult> Delete(int id)
    {
        await _mediumTypeService.DeleteMediumTypeById(id);

        return NoContent();
    }
}
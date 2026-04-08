using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telebill.Dto.Posting;
using Telebill.Services.Posting;

namespace Telebill.Controllers.Posting;

[ApiController]
[Route("api/v1/posting/payments")]
public class PaymentPostController(IPostingService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentPostRequestDto dto)
    {
        try
        {
            var result = await service.CreatePaymentPostAsync(dto, GetCurrentUserId());
            return StatusCode(201, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("by-claim/{claimID:int}")]
    public async Task<IActionResult> GetByClaim(int claimID)
    {
        try
        {
            var result = await service.GetPaymentPostsByClaimAsync(claimID);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("{paymentID:int}")]
    public async Task<IActionResult> GetById(int paymentID)
    {
        try
        {
            var result = await service.GetPaymentPostByIdAsync(paymentID);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("{paymentID:int}/void")]
    public async Task<IActionResult> Void(int paymentID, [FromBody] VoidPaymentPostRequestDto dto)
    {
        try
        {
            var result = await service.VoidPaymentPostAsync(paymentID, dto, GetCurrentUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(id, out var parsed) ? parsed : 0;
    }
}


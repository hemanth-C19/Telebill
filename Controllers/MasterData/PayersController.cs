using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;
using Telebill.Services.MasterData;

namespace Telebill.Controllers.MasterData;

[ApiController]
[Route("api/v1/MasterData/[controller]")]
public class PayersController : ControllerBase
{
    private readonly IPayerService _payerService;

    public PayersController(IPayerService payerService)
    {
        _payerService = payerService;
    }

    [HttpGet("GetAllPayers")]
    public async Task<ActionResult<IEnumerable<PayerDTO>>> GetAllPayers()
    {
        var payers = await _payerService.GetAllPayersAsync();
        return Ok(payers);
    }

    [HttpGet("GetAllPayersNames")]
    public async Task<ActionResult<IEnumerable<PayerNamesDTO>>> GetAllPayerNames()
    {
        var names = await _payerService.GetAllPayersNames();
        return Ok(names);
    }

    [HttpPost("AddPayer")]
    public async Task<IActionResult> AddPayer([FromBody] AddPayerDTO payerDto)
    {
        try
        {
            await _payerService.AddPayerAsync(payerDto);
            return Ok("Add Payer Successfull");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdatePayer")]
    public async Task<IActionResult> UpdatePayer([FromBody] UpdatePayerDTO payerDto)
    {
        try
        {
            await _payerService.UpdatePayerAsync(payerDto);
            return Ok("Update Payer Successfull");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("DeletePayer")]
    public async Task<IActionResult> DeletePayer(int payerId)
    {
        try
        {
            await _payerService.DeletePayerAsync(payerId);
            return Ok("Delete Payer Successfull");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}


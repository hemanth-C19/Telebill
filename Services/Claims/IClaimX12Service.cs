using System.Threading.Tasks;
using Telebill.Dto;

namespace Services;

public interface IClaimX12Service
{
    Task<X12RefDto?> Generate837PAsync(int claimID);
    Task<X12RefDto?> Get837PRefAsync(int claimID);
}


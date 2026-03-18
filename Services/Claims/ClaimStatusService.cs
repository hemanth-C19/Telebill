using System;
using System.Threading.Tasks;
using Telebill.Dto;
using Telebill.Repositories.Claims;

namespace Services;

public class ClaimStatusService : IClaimStatusService
{
    private readonly IClaimRepository _repo;

    public ClaimStatusService(IClaimRepository repo)
    {
        _repo = repo;
    }

    public async Task<UpdateClaimStatusResponseDto?> UpdateClaimStatusAsync(int claimID, UpdateClaimStatusRequestDto dto)
    {
        var claim = await _repo.GetByIdAsync(claimID);
        if (claim == null)
        {
            throw new KeyNotFoundException("Claim not found");
        }

        var oldStatus = claim.ClaimStatus ?? "Draft";
        var newStatus = dto.NewStatus;

        if (!IsValidStatusTransition(oldStatus, newStatus))
        {
            throw new ArgumentException($"Invalid status transition from {oldStatus} to {newStatus}");
        }

        claim.ClaimStatus = newStatus;
        await _repo.UpdateStatusAsync(claimID, newStatus);

        return new UpdateClaimStatusResponseDto
        {
            ClaimID = claimID,
            PreviousStatus = oldStatus,
            NewStatus = newStatus,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static bool IsValidStatusTransition(string oldStatus, string newStatus) =>
        newStatus switch
        {
            "ScrubError" => oldStatus is "Draft",
            "Ready" => oldStatus is "Draft" or "ScrubError",
            "Batched" => oldStatus is "Ready",
            "Submitted" => oldStatus is "Batched",
            "Accepted" or "Rejected" => oldStatus is "Submitted",
            "Paid" or "PartiallyPaid" or "Denied" => oldStatus is "Accepted",
            "Draft" => oldStatus is "Rejected",
            _ => false
        };
}


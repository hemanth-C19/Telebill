using System;
using System.Threading.Tasks;
using Telebill.Dto;
using Telebill.Models;
using Telebill.Repositories.Claims;

namespace Services;

public class ClaimX12Service(IClaimRepository repo) : IClaimX12Service
{
    public async Task<X12RefDto?> Generate837PAsync(int claimID)
    {
        var claim = await repo.GetByIdWithLinesAsync(claimID);
        if (claim == null)
        {
            throw new KeyNotFoundException("Claim not found");
        }

        if (!string.Equals(claim.ClaimStatus, "Ready", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Claim is not in Ready status");
        }

        var openErrors = await repo.CountOpenErrorsAsync(claimID);
        if (openErrors > 0)
        {
            throw new ArgumentException("Claim has open scrub errors");
        }

        if (!await repo.HasApprovedPriorAuthAsync(claimID))
        {
            throw new ArgumentException("Prior authorization required but not approved");
        }

        var uri = $"claims/837p/{DateTime.UtcNow:yyyy-MM-dd}-claim-{claimID}.edi";

        var existing = await repo.GetX12RefByClaimIDAsync(claimID);
        X12837pRef x12;

        if (existing != null)
        {
            existing.PayloadUri = uri;
            existing.GeneratedDate = DateTime.UtcNow;
            existing.Version = "005010X222A1";
            existing.Status = "Generated";
            await repo.UpdateX12RefAsync(existing);
            x12 = existing;
        }
        else
        {
            x12 = new X12837pRef
            {
                ClaimId = claimID,
                PayloadUri = uri,
                GeneratedDate = DateTime.UtcNow,
                Version = "005010X222A1",
                Status = "Generated"
            };
            x12 = await repo.CreateX12RefAsync(x12);
        }

        return new X12RefDto
        {
            X12ID = x12.X12id,
            ClaimID = claimID,
            PayloadURI = x12.PayloadUri ?? string.Empty,
            PreSignedURL = x12.PayloadUri ?? string.Empty,
            GeneratedDate = x12.GeneratedDate ?? DateTime.MinValue,
            Version = x12.Version ?? string.Empty,
            Status = x12.Status ?? string.Empty
        };
    }

    public async Task<X12RefDto?> Get837PRefAsync(int claimID)
    {
        var x12 = await repo.GetX12RefByClaimIDAsync(claimID);
        if (x12 == null)
        {
            throw new KeyNotFoundException("837P reference not found");
        }

        return new X12RefDto
        {
            X12ID = x12.X12id,
            ClaimID = x12.ClaimId ?? claimID,
            PayloadURI = x12.PayloadUri ?? string.Empty,
            PreSignedURL = x12.PayloadUri ?? string.Empty,
            GeneratedDate = x12.GeneratedDate ?? DateTime.MinValue,
            Version = x12.Version ?? string.Empty,
            Status = x12.Status ?? string.Empty
        };
    }
}


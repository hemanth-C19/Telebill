using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telebill.Dto;
using Telebill.Models;
using Telebill.Repositories.Claims;

namespace Services;

public class ClaimRuleService(IClaimRepository repo) : IClaimRuleService
{
    public async Task<List<ScrubRuleDto>> GetScrubRulesAsync(string? severity, string? status)
    {
        var rules = await repo.GetScrubRulesFilteredAsync(severity, status);
        return rules.Select(r => new ScrubRuleDto
        {
            RuleID = r.RuleId,
            Name = r.Name,
            ExpressionJSON = r.ExpressionJson ?? string.Empty,
            Severity = r.Severity ?? string.Empty,
            Status = r.Status ?? string.Empty
        }).ToList();
    }

    public async Task<ScrubRuleDto?> CreateScrubRuleAsync(CreateScrubRuleRequestDto dto)
    {
        var rule = new ScrubRule
        {
            Name = dto.Name,
            ExpressionJson = dto.ExpressionJSON,
            Severity = dto.Severity,
            Status = "Active"
        };

        var created = await repo.CreateScrubRuleAsync(rule);

        return new ScrubRuleDto
        {
            RuleID = created.RuleId,
            Name = created.Name,
            ExpressionJSON = created.ExpressionJson ?? string.Empty,
            Severity = created.Severity ?? string.Empty,
            Status = created.Status ?? string.Empty
        };
    }

    public async Task<ScrubRuleDto?> UpdateScrubRuleAsync(int ruleID, UpdateScrubRuleRequestDto dto)
    {
        var rule = await repo.GetScrubRuleByIdAsync(ruleID);
        if (rule == null)
        {
            throw new KeyNotFoundException("Scrub rule not found");
        }

        if (!string.IsNullOrWhiteSpace(dto.Name)) rule.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.ExpressionJSON)) rule.ExpressionJson = dto.ExpressionJSON;
        if (!string.IsNullOrWhiteSpace(dto.Severity)) rule.Severity = dto.Severity;
        if (!string.IsNullOrWhiteSpace(dto.Status)) rule.Status = dto.Status;

        var updated = await repo.UpdateScrubRuleAsync(rule);

        return new ScrubRuleDto
        {
            RuleID = updated.RuleId,
            Name = updated.Name,
            ExpressionJSON = updated.ExpressionJson ?? string.Empty,
            Severity = updated.Severity ?? string.Empty,
            Status = updated.Status ?? string.Empty
        };
    }

    public Task<bool> DeleteScrubRuleAsync(int ruleID)
    {
        return repo.DeleteScrubRuleAsync(ruleID);
    }
}


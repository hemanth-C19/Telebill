using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto;

namespace Services;

public interface IClaimRuleService
{
    Task<List<ScrubRuleDto>> GetScrubRulesAsync(string? severity, string? status);
    Task<ScrubRuleDto?> CreateScrubRuleAsync(CreateScrubRuleRequestDto dto);
    Task<ScrubRuleDto?> UpdateScrubRuleAsync(int ruleID, UpdateScrubRuleRequestDto dto);
    Task<bool> DeleteScrubRuleAsync(int ruleID);
}


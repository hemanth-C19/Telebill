using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Telebill.Models;

namespace Telebill.Repositories.Claims;

public partial class ClaimRepository
{
    public Task<List<ScrubRule>> GetActiveScrubRulesAsync()
    {
        return context.ScrubRules
            .Where(r => r.Status == "Active")
            .ToListAsync();
    }

    public Task<List<ScrubRule>> GetScrubRulesFilteredAsync(string? severity, string? status)
    {
        var query = context.ScrubRules.AsQueryable();

        if (!string.IsNullOrWhiteSpace(severity))
        {
            query = query.Where(r => r.Severity == severity);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        return query.ToListAsync();
    }

    public Task<ScrubRule?> GetScrubRuleByIdAsync(int ruleID)
    {
        return context.ScrubRules.FirstOrDefaultAsync(r => r.RuleId == ruleID);
    }

    public async Task<ScrubRule> CreateScrubRuleAsync(ScrubRule rule)
    {
        context.ScrubRules.Add(rule);
        await context.SaveChangesAsync();
        return rule;
    }

    public async Task<ScrubRule> UpdateScrubRuleAsync(ScrubRule rule)
    {
        context.ScrubRules.Update(rule);
        await context.SaveChangesAsync();
        return rule;
    }

    public async Task<bool> DeleteScrubRuleAsync(int ruleID)
    {
        var rule = await context.ScrubRules.FindAsync(ruleID);
        if (rule == null) return false;
        context.ScrubRules.Remove(rule);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ScrubIssue>> GetIssuesByClaimIDAsync(int claimID, string statusFilter)
    {
        var query = context.ScrubIssues
            .Include(i => i.Rule)
            .Include(i => i.ClaimLine)
            .Where(i => i.ClaimId == claimID);

        if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all")
        {
            query = query.Where(i => i.Status == statusFilter);
        }

        return await query.ToListAsync();
    }

    public Task<ScrubIssue?> GetIssueByIdAsync(int issueID)
    {
        return context.ScrubIssues.FirstOrDefaultAsync(i => i.IssueId == issueID);
    }

    public Task<ScrubIssue?> GetOpenIssueByRuleAsync(int claimID, int ruleID, int? claimLineID)
    {
        return context.ScrubIssues.FirstOrDefaultAsync(i =>
            i.ClaimId == claimID &&
            i.RuleId == ruleID &&
            i.Status == "Open" &&
            ((claimLineID == null && i.ClaimLineId == null) ||
             (claimLineID != null && i.ClaimLineId == claimLineID)));
    }

    public async Task<int> CountOpenErrorsAsync(int claimID)
    {
        return await context.ScrubIssues
            .Join(context.ScrubRules, i => i.RuleId, r => r.RuleId, (i, r) => new { i, r })
            .CountAsync(j => j.i.ClaimId == claimID && j.i.Status == "Open" && j.r.Severity == "Error");
    }

    public async Task<int> CountOpenWarningsAsync(int claimID)
    {
        return await context.ScrubIssues
            .Join(context.ScrubRules, i => i.RuleId, r => r.RuleId, (i, r) => new { i, r })
            .CountAsync(j => j.i.ClaimId == claimID && j.i.Status == "Open" && j.r.Severity == "Warning");
    }

    public async Task CreateIssueAsync(ScrubIssue issue)
    {
        context.ScrubIssues.Add(issue);
        await context.SaveChangesAsync();
    }

    public async Task ResolveIssueAsync(int issueID)
    {
        var issue = await context.ScrubIssues.FindAsync(issueID);
        if (issue == null)
        {
            return;
        }

        issue.Status = "Resolved";
        await context.SaveChangesAsync();
    }

    public async Task ResolveIssueByRuleAsync(int claimID, int ruleID, int? claimLineID)
    {
        var issues = await context.ScrubIssues
            .Where(i =>
                i.ClaimId == claimID &&
                i.RuleId == ruleID &&
                i.Status == "Open" &&
                ((claimLineID == null && i.ClaimLineId == null) ||
                 (claimLineID != null && i.ClaimLineId == claimLineID)))
            .ToListAsync();

        foreach (var issue in issues)
        {
            issue.Status = "Resolved";
        }

        await context.SaveChangesAsync();
    }
}


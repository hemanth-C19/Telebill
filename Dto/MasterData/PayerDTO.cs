using System;

namespace Telebill.Dto.MasterData
{
    public class PayerDTO
    {
        public string Name { get; set; } = null!;
        public string? PayerCode { get; set; }
        public string? ClearinghouseCode { get; set; }
        public string? ContactInfo { get; set; }
        public string? Status { get; set; }
    }

    public class PayerPlanDTO
    {
        public int? PayerId { get; set; }

        public string PlanName { get; set; } = null!;

        public string? NetworkType { get; set; }

        public string? Posdefault { get; set; }

        public string? TelehealthModifiersJson { get; set; }

        public string? Status { get; set; }
    }

    public class FeeDTO
    {
        public int? PlanId { get; set; }

        public string CptHcpcs { get; set; } = null!;

        public string? ModifierCombo { get; set; }

        public decimal? AllowedAmount { get; set; }

        public DateOnly? EffectiveFrom { get; set; }

        public DateOnly? EffectiveTo { get; set; }

        public string? Status { get; set; }

    }

    public class PayerNamesDTO
    {
        public int PayerId { get; set; }
        public string PayerName { get; set; }
        public string PayerCode { get; set; }
    }

    public class PlanNamesDTO
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; }
    }
}
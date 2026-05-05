namespace Telebill.Validations.MasterData;

/// <summary>Provider, Payer, PayerPlan, FeeSchedule lifecycle (Document 2).</summary>
public enum MasterEntityStatus
{
    Active,
    Inactive
}

/// <summary>PayerPlan.NetworkType (Document 2).</summary>
public enum PlanNetworkType
{
    HMO,
    PPO,
    Medicare,
    Medicaid,
    Commercial
}

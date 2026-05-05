namespace Telebill.Dto.Posting;

public class PatientBalanceFilterParams
{
    public int? PatientID { get; set; }
    public string? AgingBucket { get; set; }
    public string? Status { get; set; } = "Open";
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}


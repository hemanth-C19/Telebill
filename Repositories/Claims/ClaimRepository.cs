using Telebill.Data;

namespace Telebill.Repositories.Claims;

public partial class ClaimRepository : IClaimRepository
{
    private readonly TeleBillContext _context;

    public ClaimRepository(TeleBillContext context)
    {
        _context = context;
    }
}


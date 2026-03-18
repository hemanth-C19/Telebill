using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.AR;

namespace Telebill.Services.AR;

public interface IUnderpaymentService
{
    Task<List<UnderpaymentItemDto>> GetUnderpaymentWorklistAsync();
    Task<(bool success, string error)> FlagUnderpaymentAsync(FlagUnderpaymentDto dto, int userId);
}

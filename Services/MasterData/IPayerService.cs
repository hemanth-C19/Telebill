using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telebill.Dto.MasterData;

namespace Telebill.Services.MasterData
{
    public interface IPayerService
    {
        Task<IEnumerable<PayerNamesDTO>> GetAllPayersNames();
        Task<IEnumerable<PayerDTO>> GetAllPayersAsync();

        Task AddPayerAsync(PayerDTO payerDto);
        Task UpdatePayerAsync(PayerDTO payerDto);
        Task DeletePayerAsync(int payerId);
    }
}
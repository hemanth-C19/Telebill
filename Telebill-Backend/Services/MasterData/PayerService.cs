using System;
using System.Collections.Generic;
using System.Linq;
using Telebill.Dto.MasterData;
using Telebill.Models;
using Telebill.Repositories.MasterData;

namespace Telebill.Services.MasterData
{
    public class PayerService : IPayerService
    {
        private readonly IPayerRepository _payerRepository;

        public PayerService(IPayerRepository payerRepository)
        {
            _payerRepository = payerRepository;
        }

        public async Task<IEnumerable<PayerDTO>> GetAllPayersAsync(string? search, int page, int limit)
        {
            var payers = await _payerRepository.GetAllAsync(search, page, limit);
            return payers.Select(p => new PayerDTO
            {
                PayerId = p.PayerId,
                Name = p.Name,
                PayerCode = p.PayerCode,
                ClearinghouseCode = p.ClearinghouseCode,
                ContactInfo = p.ContactInfo,
                Status = p.Status
            });
        }

        public async Task<IEnumerable<PayerNamesDTO>> GetAllPayersNames()
        {
            var payers = await _payerRepository.GetPayerNamesAsync();
            return payers.Select(p => new PayerNamesDTO
            {
                PayerId = p.PayerId,
                PayerName = p.PayerName,
                PayerCode = p.PayerCode ?? string.Empty
            });
        }

        public async Task AddPayerAsync(AddPayerDTO payerDto)
        {
            if (string.IsNullOrWhiteSpace(payerDto.Name))
                throw new ArgumentException("Payer name is required.");

            var entity = new Payer
            {
                Name = payerDto.Name,
                PayerCode = payerDto.PayerCode,
                ClearinghouseCode = payerDto.ClearinghouseCode,
                ContactInfo = payerDto.ContactInfo,
                Status = payerDto.Status
            };

            await _payerRepository.AddAsync(entity);
        }

        public async Task UpdatePayerAsync(UpdatePayerDTO payerDto)
        {
            if (!payerDto.PayerId.HasValue)
                throw new ArgumentException("PayerId is required for update.");

            var existing = await _payerRepository.GetPayerByIdAsync(payerDto.PayerId.Value);
            if (existing == null)
                throw new KeyNotFoundException($"Payer {payerDto.PayerId.Value} not found.");

            if (string.IsNullOrWhiteSpace(payerDto.Name))
                throw new ArgumentException("Payer name is required.");

            existing.Name = payerDto.Name;
            existing.PayerCode = payerDto.PayerCode;
            existing.ClearinghouseCode = payerDto.ClearinghouseCode;
            existing.ContactInfo = payerDto.ContactInfo;
            existing.Status = payerDto.Status;

            await _payerRepository.UpdateAsync(existing);
        }

        public async Task DeletePayerAsync(int payerId)
        {
            var existing = await _payerRepository.GetPayerByIdAsync(payerId);
            if (existing == null)
                throw new KeyNotFoundException($"Payer {payerId} not found.");

            await _payerRepository.DeleteAsync(existing);
        }
    }
}
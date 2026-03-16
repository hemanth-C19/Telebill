using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Telebill.Dto.MasterData
{
    public class ProviderDTO
    {
        public int ProviderId { get; set; }

        public string Npi { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string? Taxonomy { get; set; }

        public bool? TelehealthEnrolled { get; set; }

        public string? ContactInfo { get; set; }

        public string? Status { get; set; }
    }

    public class ProviderActiveInfo()
    {
        public int ProviderId { get; set; }
        public string? ProviderName { get; set; }
    }

    public class CreateUpdateProviderDTO()
    {
        public string? ProviderName {get; set;}
        public string? ProviderNpi {get; set;}
        public string? ProviderTaxonomy {get; set;}
        public bool ProviderEnrolled {get; set;}
        public string? ProviderContact {get; set;}
        public string? ProviderStatus {get; set;}
    }
}
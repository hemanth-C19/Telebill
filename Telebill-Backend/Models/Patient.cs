using System;
using System.Collections.Generic;

namespace Telebill.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string Mrn { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateOnly Dob { get; set; }

    public string? Gender { get; set; }

    public string? AddressJson { get; set; }

    public string? ContactInfo { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    public virtual ICollection<Coverage> Coverages { get; set; } = new List<Coverage>();

    public virtual ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();

    public virtual ICollection<PatientBalance> PatientBalances { get; set; } = new List<PatientBalance>();

    public virtual ICollection<Statement> Statements { get; set; } = new List<Statement>();
}

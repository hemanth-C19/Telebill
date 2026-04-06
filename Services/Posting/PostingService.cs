using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Telebill.Dto.Posting;
using Telebill.Models;
using Telebill.Repositories.Posting;

namespace Telebill.Services.Posting;

public partial class PostingService : IPostingService
{
    private readonly IPostingRepository _repo;
    private readonly IConfiguration _config;

    private static readonly HashSet<string> DenialCarcCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "4","16","50","96","97","181","CO97","CO4","CO50"
    };

    public PostingService(IPostingRepository repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    // Shared helpers are implemented in the other partial files.
}


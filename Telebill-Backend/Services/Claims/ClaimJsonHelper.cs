using System.Collections.Generic;
using System.Text.Json;

namespace Services;

internal static class ClaimJsonHelper
{
    public static List<string> SafeDeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static List<int> SafeDeserializeIntList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<int>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch
        {
            return new List<int>();
        }
    }
}


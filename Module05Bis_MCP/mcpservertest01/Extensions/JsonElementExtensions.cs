using System.Text.Json;

namespace mcpservertest01.Extensions;

/// <summary>
/// Extensions pour faciliter la manipulation de JsonElement
/// </summary>
public static class JsonElementExtensions
{
    /// <summary>
    /// Convertit un JsonElement en objet dynamique
    /// </summary>
    public static object? ToObject(this JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.ToObject()),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(item => item.ToObject())
                .ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out long l) ? l :
                                   element.TryGetDouble(out double d) ? d :
                                   element.GetRawText(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null or JsonValueKind.Undefined or _ => null,
        };
    }

    /// <summary>
    /// Tente d'obtenir une propriété de manière sécurisée
    /// </summary>
    public static bool TryGetProperty(this JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out JsonElement prop))
        {
            value = prop;
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Obtient une propriété string de manière sécurisée
    /// </summary>
    public static string? GetPropertyString(this JsonElement element, string propertyName, string? defaultValue = null)
    {
        return element.TryGetProperty(propertyName, out JsonElement prop) 
            ? prop.GetString() ?? defaultValue 
            : defaultValue;
    }

    /// <summary>
    /// Obtient une propriété int de manière sécurisée
    /// </summary>
    public static int GetPropertyInt32(this JsonElement element, string propertyName, int defaultValue = 0)
    {
        return element.TryGetProperty(propertyName, out JsonElement prop) && prop.TryGetInt32(out int value)
            ? value
            : defaultValue;
    }
}

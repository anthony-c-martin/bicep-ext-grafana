using System.Text.Json;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Bicep.Extension.Grafana.Handlers;

internal static class KiotaJson
{
    public static Dictionary<string, object> ParseObject(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Grafana JSON values must contain a JSON object.");
        }

        return document.RootElement.EnumerateObject().ToDictionary(
            property => property.Name,
            property => (object)ToUntypedNode(property.Value),
            StringComparer.Ordinal);
    }

    private static UntypedNode ToUntypedNode(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.Object => new UntypedObject(element.EnumerateObject().ToDictionary(
                property => property.Name,
                property => ToUntypedNode(property.Value),
                StringComparer.Ordinal)),
            JsonValueKind.Array => new UntypedArray([.. element.EnumerateArray().Select(ToUntypedNode)]),
            JsonValueKind.String => new UntypedString(element.GetString()!),
            JsonValueKind.Number when element.TryGetInt64(out var value) => new UntypedLong(value),
            JsonValueKind.Number => new UntypedDouble(element.GetDouble()),
            JsonValueKind.True => new UntypedBoolean(true),
            JsonValueKind.False => new UntypedBoolean(false),
            JsonValueKind.Null => new UntypedNull(),
            _ => throw new JsonException($"Unsupported JSON value kind '{element.ValueKind}'."),
        };
}

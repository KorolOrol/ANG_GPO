using System;
using System.Text.Json;

namespace Cassius2.Helpers
{
    public static class ValueParser
    {
        public static object? Parse(string input, Type type)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            // Direct parsing strategy for primitives and simple types
            if (type == typeof(string)) return input;
            if (type == typeof(bool) && bool.TryParse(input, out var b)) return b;
            if (type == typeof(int) && int.TryParse(input, out var i)) return i;
            if (type == typeof(double) && double.TryParse(input, out var d)) return d;
            if (type == typeof(DateTime) && DateTime.TryParse(input, out var dt)) return dt;
            if (type == typeof(Guid) && Guid.TryParse(input, out var g)) return g;
            
            // For other simple types, try ChangeType
            if (type.IsPrimitive || type.IsEnum || type == typeof(decimal))
            {
                try { return Convert.ChangeType(input, type); } catch { }
            }

            // Fallback for List, Dictionary, or complex JSON
            return JsonSerializer.Deserialize(input, type);
        }
    }
}


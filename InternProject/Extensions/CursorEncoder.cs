using InternProject.Models.PagingModels;
using System.Text;
using System.Text.Json;

namespace InternProject.Extensions
{
    public static class CursorEncoder
    {
        public static string? Encode(object? cursor)
        {
            if (cursor == null) return null;

            var json = JsonSerializer.Serialize(cursor);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public static CompositeCursor<TPrimary, TSecondary>? Decode<TPrimary, TSecondary>(string? cursor)
            where TPrimary : IComparable
            where TSecondary : IComparable
        {
            if (string.IsNullOrWhiteSpace(cursor))
                return null;

            try
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                return JsonSerializer.Deserialize<CompositeCursor<TPrimary, TSecondary>>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}

using Microsoft.OpenApi;

namespace InternProject.Models
{
    public class ApiResponse<T>(bool status, string message, T data, Object? meta = null)
    {
        public bool Status { get; set; } = status;
        public string? Message { get; set; } = message;
        public T? Data { get; set; } = data; 
        public Object? Meta { get; set; } = meta;
    }
}

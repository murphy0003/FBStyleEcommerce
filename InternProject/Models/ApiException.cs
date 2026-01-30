namespace InternProject.Models
{
    public class ApiException(String Code , object? Details , int StatusCode) : Exception
    {
        public string Code { get; set; } = Code;
        public object? Details { get; set; } = Details;
        public int StatusCode { get; set; } = StatusCode;
    }
}

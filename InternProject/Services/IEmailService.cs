namespace InternProject.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string email, string otp , CancellationToken cancellationToken);
    }
}

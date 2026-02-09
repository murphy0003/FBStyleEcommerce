namespace InternProject.Services.EmailService
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string email, string otp , CancellationToken cancellationToken);
    }
}

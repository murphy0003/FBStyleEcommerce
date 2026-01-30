
using MailKit.Net.Smtp;
using MimeKit;

namespace InternProject.Services
{
    public class EmailService(IConfiguration config) : IEmailService
    {
        public async Task SendOtpEmailAsync(string recipientEmail, string otp , CancellationToken cancellationToken)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(config["SmtpSettings:SenderName"], config["SmtpSettings:SenderEmail"]));
            message.To.Add(new MailboxAddress("", recipientEmail));
            message.Subject = "Verify Your Account - OTP Code";

            // Professional HTML Body
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; border: 1px solid #ddd; padding: 20px;'>
                    <h2>Welcome to Our App!</h2>
                    <p>Please use the code below to verify your email address:</p>
                    <h1 style='color: #007bff;'>{otp}</h1>
                    <p>This code will expire in 15 minutes.</p>
                </div>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(config["SmtpSettings:Server"],
                                        int.Parse(config["SmtpSettings:Port"]),
                                        MailKit.Security.SecureSocketOptions.StartTls,cancellationToken);

                await client.AuthenticateAsync(config["SmtpSettings:Username"], config["SmtpSettings:Password"],cancellationToken);

                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}

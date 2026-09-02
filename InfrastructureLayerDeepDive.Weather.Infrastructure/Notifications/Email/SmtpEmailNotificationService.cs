using InfrastructureLayerDeepDive.Weather.Application.InfrastructureContracts;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace InfrastructureLayerDeepDive.Weather.Infrastructure.Notifications.Email
{
    public class SmtpEmailNotificationService(
        IOptions<SmtpOptions> options,
        ILogger<SmtpEmailNotificationService> logger) : INotificationService<EmailMessage>
    {
        private readonly SmtpOptions _options = options.Value;

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            mimeMessage.To.Add(MailboxAddress.Parse(message.ToAddress));
            mimeMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder();
            if (message.IsHtml)
            {
                bodyBuilder.HtmlBody = message.Body;
            }
            else
            {
                bodyBuilder.TextBody = message.Body;
            }
            foreach (var attachment in message.Attachments)
            {
                await bodyBuilder.Attachments.AddAsync(attachment.FileName, attachment.FileStream, cancellationToken: cancellationToken);
            }
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            if (!string.IsNullOrEmpty(message.CcAddress))
                mimeMessage.Cc.Add(new MailboxAddress(string.Empty, message.CcAddress));

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_options.Host, _options.Port,
                    _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                    cancellationToken);
                client.CheckCertificateRevocation = false;
                client.Timeout = (int)TimeSpan.FromSeconds(10000).TotalMilliseconds;

                if (!string.IsNullOrWhiteSpace(_options.Username))
                {
                    await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
                }

                await client.SendAsync(mimeMessage, cancellationToken);
                logger.LogInformation("Email sent to {ToAddress} with subject {Subject}", message.ToAddress, message.Subject);
            }
            finally
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}
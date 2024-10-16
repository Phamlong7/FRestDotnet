
using System.Net;
using System.Net.Mail;
using Restaurant.Utility;


namespace Restaurant.ViewModels
{
    public class SendMail
    {
        private readonly ConstantHelper _constantHelper;
        private readonly ILogger<SendMail> _logger;

        public SendMail(ConstantHelper constantHelper, ILogger<SendMail> logger)
        {
            _constantHelper = constantHelper ?? throw new ArgumentNullException(nameof(constantHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Method to send a regular email
        public async Task<bool> SendEmailAsync(string to, string subject, string body, string? attachmentFile = null)
        {
            try
            {
                _logger.LogInformation("Attempting to send email to {Recipient}", to);
                _logger.LogDebug("SMTP Host: {Host}, Port: {Port}", _constantHelper.hostemail, _constantHelper.port);

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(_constantHelper.emailsender);
                    mail.To.Add(to);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    if (!string.IsNullOrEmpty(attachmentFile))
                    {
                        mail.Attachments.Add(new Attachment(attachmentFile));
                        _logger.LogDebug("Attachment added: {AttachmentFile}", attachmentFile);
                    }

                    using (var smtpClient = new SmtpClient(_constantHelper.hostemail))
                    {
                        smtpClient.Port = _constantHelper.port;
                        smtpClient.Credentials = new NetworkCredential(_constantHelper.emailsender, _constantHelper.paswordsender);
                        smtpClient.EnableSsl = true;

                        _logger.LogDebug("Sending email...");
                        await smtpClient.SendMailAsync(mail);
                        _logger.LogDebug("Email sent successfully");
                    }
                }

                _logger.LogInformation("Email sent successfully to {Recipient}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}. Error: {ErrorMessage}", to, ex.Message);
                return false;
            }
        }
    }
}

using System.Net;
using System.Net.Mail;
using EmailScheduler.Models;
using Microsoft.Extensions.Options;

namespace EmailScheduler.Services
{
    /// <summary>
    /// Implementation of the email service
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private DateTime _lastSendTime = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the EmailService class
        /// </summary>
        /// <param name="emailSettings">Email configuration settings</param>
        /// <param name="logger">Logger instance</param>
        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Sends account activation emails to all recipients
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task SendAccountActivationEmailsAsync()
        {
            if (_emailSettings.Recipients == null || !_emailSettings.Recipients.Any())
            {
                _logger.LogWarning("No recipients configured for account activation emails");
                return;
            }

            _logger.LogInformation(
                "Starting email sending process at {Time} with {Count} recipients",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                _emailSettings.Recipients.Count
            );

            try
            {
                using var client = CreateSmtpClient();
                _logger.LogInformation(
                    "Connected to SMTP server {Server}:{Port}",
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort
                );

                int successCount = 0;
                int failureCount = 0;

                foreach (var recipient in _emailSettings.Recipients)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(recipient))
                        {
                            _logger.LogWarning("Skipping empty or null recipient email address");
                            continue;
                        }

                        var message = CreateEmailMessage(recipient);
                        await client.SendMailAsync(message);
                        successCount++;
                        _logger.LogInformation(
                            "✓ Email successfully sent to {Recipient} at {Time}",
                            recipient,
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        );
                    }
                    catch (SmtpException smtpEx)
                    {
                        failureCount++;
                        _logger.LogError(
                            smtpEx,
                            "✗ SMTP error sending email to {Recipient}: {ErrorMessage} (Status: {Status})",
                            recipient,
                            smtpEx.Message,
                            smtpEx.StatusCode
                        );
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(
                            ex,
                            "✗ Failed to send email to {Recipient}: {ErrorMessage}",
                            recipient,
                            ex.Message
                        );
                    }
                }

                _lastSendTime = DateTime.Now;
                _logger.LogInformation(
                    "Email sending process completed. Success: {SuccessCount}, Failures: {FailureCount}",
                    successCount,
                    failureCount
                );
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(
                    smtpEx,
                    "SMTP error connecting to server {Server}:{Port}: {ErrorMessage} (Status: {Status})",
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    smtpEx.Message,
                    smtpEx.StatusCode
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error connecting to SMTP server {Server}:{Port}: {ErrorMessage}",
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    ex.Message
                );
            }
        }

        /// <summary>
        /// Checks if it's time to send emails based on the configured schedule
        /// </summary>
        /// <returns>True if emails should be sent now, otherwise false</returns>
        public bool ShouldSendEmails()
        {
            var now = DateTime.Now;

            // For testing: send every X minutes
            if (_emailSettings.UseTestSchedule)
            {
                var minutesSinceLastSend = (now - _lastSendTime).TotalMinutes;
                var shouldSend = minutesSinceLastSend >= _emailSettings.TestIntervalMinutes;

                if (shouldSend)
                {
                    _logger.LogInformation(
                        "Test schedule triggered: {Minutes} minutes elapsed since last send (threshold: {Threshold})",
                        Math.Round(minutesSinceLastSend, 2),
                        _emailSettings.TestIntervalMinutes
                    );
                }

                return shouldSend;
            }

            // For production: send weekly on configured day/hour
            var isWeeklyScheduleTime =
                now.DayOfWeek == (DayOfWeek)_emailSettings.WeeklyScheduleDay
                && now.Hour == _emailSettings.WeeklyScheduleHour
                && now.Minute == 0
                && (now - _lastSendTime).TotalHours >= 1;

            if (isWeeklyScheduleTime)
            {
                _logger.LogInformation(
                    "Weekly schedule triggered: Day {Day}, Hour {Hour}",
                    now.DayOfWeek,
                    now.Hour
                );
            }

            return isWeeklyScheduleTime;
        }

        /// <summary>
        /// Creates an SMTP client configured with the application settings
        /// </summary>
        /// <returns>Configured SmtpClient instance</returns>
        private SmtpClient CreateSmtpClient()
        {
            try
            {
                var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

                // Setup credentials if provided
                if (
                    !string.IsNullOrEmpty(_emailSettings.Username)
                    && !string.IsNullOrEmpty(_emailSettings.Password)
                )
                {
                    client.Credentials = new NetworkCredential(
                        _emailSettings.Username,
                        _emailSettings.Password
                    );
                    _logger.LogDebug(
                        "Using credentials for user {Username}",
                        _emailSettings.Username
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "No SMTP credentials provided - anonymous authentication will be used"
                    );
                }

                return client;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create SMTP client: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates an email message for account activation
        /// </summary>
        /// <param name="recipient">Email recipient address</param>
        /// <returns>MailMessage configured for sending</returns>
        private MailMessage CreateEmailMessage(string recipient)
        {
            try
            {
                // Validate sender email
                if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail))
                {
                    _logger.LogError("Sender email is not configured properly");
                    throw new InvalidOperationException("Sender email is not configured");
                }

                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = "Account Activation Reminder",
                    Body =
                        $"This is an automated email to keep your account active. No action is required.\n\nSent at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    IsBodyHtml = false,
                };

                message.To.Add(recipient);
                _logger.LogDebug(
                    "Created email message to {Recipient} with subject '{Subject}'",
                    recipient,
                    message.Subject
                );

                return message;
            }
            catch (FormatException ex)
            {
                _logger.LogError(
                    ex,
                    "Invalid email format. Sender: '{Sender}', Recipient: '{Recipient}'",
                    _emailSettings.SenderEmail,
                    recipient
                );
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating email message: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}

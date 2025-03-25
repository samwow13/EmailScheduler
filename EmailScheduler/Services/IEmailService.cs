namespace EmailScheduler.Services
{
    /// <summary>
    /// Interface for email service functionality
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email to keep user accounts active
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SendAccountActivationEmailsAsync();

        /// <summary>
        /// Checks if it's time to send emails based on the configured schedule
        /// </summary>
        /// <returns>True if emails should be sent now, otherwise false</returns>
        bool ShouldSendEmails();
    }
}

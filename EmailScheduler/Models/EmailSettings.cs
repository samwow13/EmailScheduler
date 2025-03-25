namespace EmailScheduler.Models
{
    /// <summary>
    /// Configuration model for email settings
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// List of email recipients
        /// </summary>
        public List<string> Recipients { get; set; } = new List<string>();
        
        /// <summary>
        /// SMTP server settings
        /// </summary>
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        
        /// <summary>
        /// SMTP port
        /// </summary>
        public int SmtpPort { get; set; } = 587;
        
        /// <summary>
        /// Sender email address
        /// </summary>
        public string SenderEmail { get; set; } = "noreply@example.com";
        
        /// <summary>
        /// Sender name
        /// </summary>
        public string SenderName { get; set; } = "Email Scheduler Service";
        
        /// <summary>
        /// Email username for authentication
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Email password for authentication
        /// </summary>
        public string Password { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether to use SSL for the SMTP connection
        /// </summary>
        public bool EnableSsl { get; set; } = true;
        
        /// <summary>
        /// Schedule interval in minutes for testing
        /// </summary>
        public int TestIntervalMinutes { get; set; } = 1;
        
        /// <summary>
        /// Day of week for sending in production (0 = Sunday, 1 = Monday, etc.)
        /// </summary>
        public int WeeklyScheduleDay { get; set; } = 1; // Monday
        
        /// <summary>
        /// Hour of day for sending in production (24-hour format)
        /// </summary>
        public int WeeklyScheduleHour { get; set; } = 9; // 9 AM
        
        /// <summary>
        /// Whether to use test schedule (every X minutes) or production schedule (weekly)
        /// </summary>
        public bool UseTestSchedule { get; set; }
    }
}

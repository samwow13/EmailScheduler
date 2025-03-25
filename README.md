# Email Scheduler Microservice

A lightweight .NET microservice application for scheduling and sending automated emails at regular intervals. This service is designed to help keep accounts active by sending periodic emails to a configured list of recipients.

## Features

- **Flexible Scheduling**: Configure emails to be sent on a weekly schedule or at regular intervals for testing
- **SMTP Support**: Works with any SMTP server, including Gmail, Outlook, and others
- **Configurable Recipients**: Easily manage a list of email recipients
- **Detailed Logging**: Comprehensive logging of all email operations
- **Background Service**: Runs as a Windows service in the background

## Project Structure

```
EmailMicroServiceApp/
├── EmailScheduler/           # Main application directory
│   ├── Models/               # Data models
│   │   └── EmailSettings.cs  # Email configuration model
│   ├── Services/             # Service implementations
│   │   ├── EmailService.cs   # Email sending service
│   │   └── IEmailService.cs  # Email service interface
│   ├── Worker.cs             # Background service worker
│   ├── Program.cs            # Application entry point
│   └── appsettings.json      # Application configuration
```

## Setup Instructions

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- SMTP server access (can use Gmail, Outlook, or other email providers)

### Installation

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/EmailMicroServiceApp.git
   cd EmailMicroServiceApp
   ```

2. Build the application:
   ```
   cd EmailScheduler
   dotnet build
   ```

3. Configure the application (see Configuration section below)

4. Run the application:
   ```
   dotnet run
   ```

## Configuration

All configuration is done through the `appsettings.json` file in the EmailScheduler directory.

### Email Settings

Edit the `EmailSettings` section in `appsettings.json`:

```json
"EmailSettings": {
  "Recipients": ["user1@example.com", "user2@example.com"],
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderName": "Email Scheduler Service",
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "EnableSsl": true,
  "TestIntervalMinutes": 60,
  "WeeklyScheduleDay": 1,
  "WeeklyScheduleHour": 9,
  "UseTestSchedule": false
}
```

### Adding Email Recipients

To add email recipients, modify the `Recipients` array in the `EmailSettings` section:

```json
"Recipients": [
  "user1@example.com",
  "user2@example.com",
  "user3@example.com"
]
```

### Scheduling Configuration

The application supports two scheduling modes:

#### Test Schedule

For testing purposes, you can configure the service to send emails at regular intervals:

```json
"TestIntervalMinutes": 60,  // Send emails every 60 minutes
"UseTestSchedule": true     // Enable test schedule mode
```

#### Weekly Schedule (Production)

For production use, you can configure the service to send emails on a specific day and time each week:

```json
"WeeklyScheduleDay": 1,     // 0 = Sunday, 1 = Monday, etc.
"WeeklyScheduleHour": 9,    // 9 AM in 24-hour format
"UseTestSchedule": false    // Disable test schedule, use weekly schedule
```

### SMTP Configuration

To configure your SMTP server:

1. Set the SMTP server address and port:
   ```json
   "SmtpServer": "smtp.gmail.com",
   "SmtpPort": 587
   ```

2. Set your sender email and display name:
   ```json
   "SenderEmail": "your-email@gmail.com",
   "SenderName": "Email Scheduler Service"
   ```

3. Set your authentication credentials:
   ```json
   "Username": "your-email@gmail.com",
   "Password": "your-app-password"
   ```

   **Note for Gmail users**: You'll need to use an App Password instead of your regular password. See [Google's App Passwords](https://support.google.com/accounts/answer/185833) for more information.

4. Enable or disable SSL:
   ```json
   "EnableSsl": true
   ```

## Logging

The application uses NLog for logging. Logs are stored in the `logs` directory within the application folder.

## Running as a Windows Service

To install the application as a Windows service:

1. Publish the application:
   ```
   dotnet publish -c Release -o C:\EmailScheduler
   ```

2. Install the service using the Windows Service Control Manager:
   ```
   sc create EmailScheduler binPath= "C:\EmailScheduler\EmailScheduler.exe"
   sc description EmailScheduler "Email scheduling service for account activation"
   sc start EmailScheduler
   ```

## Troubleshooting

### Common Issues

1. **SMTP Authentication Failures**:
   - Verify your username and password are correct
   - For Gmail, ensure you're using an App Password
   - Check that "Less secure app access" is enabled (if applicable)

2. **Emails Not Sending**:
   - Check the logs for detailed error messages
   - Verify your SMTP server and port settings
   - Ensure your sender email is properly formatted

3. **Service Not Starting**:
   - Check the Windows Event Log for startup errors
   - Verify the service account has proper permissions

## License

[MIT License](LICENSE)

using NLog;
using EmailScheduler.Services;
using Microsoft.Extensions.Options;
using EmailScheduler.Models;
using System.Collections.Generic;

namespace EmailScheduler;

/// <summary>
/// Worker service that performs email scheduling tasks
/// </summary>
public class Worker : BackgroundService
{
    private readonly Logger _logger;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private int _checkCount = 0;

    /// <summary>
    /// Initializes a new instance of the Worker class
    /// </summary>
    /// <param name="emailService">Email service for sending emails</param>
    /// <param name="emailSettings">Email configuration settings</param>
    public Worker(IEmailService emailService, IOptions<EmailSettings> emailSettings)
    {
        _logger = LogManager.GetCurrentClassLogger();
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
    }

    /// <summary>
    /// Executes the worker process
    /// </summary>
    /// <param name="stoppingToken">Token to monitor for cancellation requests</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Info("📧 Email Scheduler service started at: {time}", DateTimeOffset.Now);
        _logger.Info("📋 Configured recipients: {recipients}", string.Join(", ", _emailSettings.Recipients ?? new List<string>()));
        
        if (_emailSettings.UseTestSchedule)
        {
            _logger.Info("⏱️ Running in TEST mode - emails will be sent every {minutes} minute(s)", _emailSettings.TestIntervalMinutes);
        }
        else
        {
            _logger.Info("📅 Running in PRODUCTION mode - emails will be sent weekly on {day} at {hour}:00", 
                (DayOfWeek)_emailSettings.WeeklyScheduleDay, _emailSettings.WeeklyScheduleHour);
        }
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _checkCount++;
                _logger.Info("🔍 Schedule check #{count} at: {time}", _checkCount, DateTimeOffset.Now);
                
                // Check if it's time to send emails
                if (_emailService.ShouldSendEmails())
                {
                    _logger.Info("📤 Sending account activation emails to {count} recipients", 
                        _emailSettings.Recipients?.Count ?? 0);
                    
                    var startTime = DateTime.Now;
                    await _emailService.SendAccountActivationEmailsAsync();
                    var duration = (DateTime.Now - startTime).TotalSeconds;
                    
                    _logger.Info("✅ Email sending process completed in {seconds} seconds", Math.Round(duration, 2));
                }
                else
                {
                    _logger.Info("⏳ Not time to send emails yet - waiting for next check");
                }
                
                // Wait one minute before checking again
                _logger.Debug("💤 Sleeping for 1 minute before next schedule check");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, don't log as error
            _logger.Info("🛑 Email Scheduler service was canceled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "❌ Exception in Email Scheduler service: {message}", ex.Message);
        }
        
        _logger.Info("👋 Email Scheduler service stopping at: {time}", DateTimeOffset.Now);
    }
}

using HealthCareAppointmentSystem.Data;
using HealthCareAppointmentSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAppointmentSystem.Services
{
    public class AppointmentReminderService : BackgroundService
    {
        private readonly ILogger<AppointmentReminderService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public AppointmentReminderService(ILogger<AppointmentReminderService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AppointmentReminderService is starting.");

            // Running every hour
            using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

            // To allow immediate testing, we'll run it once immediately before entering the timer loop.
            try
            {
                await ProcessRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing AppointmentReminderService on initial run.");
            }

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing AppointmentReminderService.");
                }
            }
        }

        private async Task ProcessRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;
            var targetTimeStart = now.AddHours(23);
            var targetTimeEnd = now.AddHours(25);

            var upcomingAppointments = await context.Appointments
                .Include(a => a.Patient)
                .ThenInclude(p => p.ApplicationUser)
                .Where(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed)
                .Where(a => a.AppointmentDateTime >= targetTimeStart && a.AppointmentDateTime <= targetTimeEnd)
                .ToListAsync();

            foreach (var appointment in upcomingAppointments)
            {
                var alreadySent = await context.AuditLogs.AnyAsync(l => 
                    l.Action == "Appointment Reminder" && 
                    l.Details.Contains(appointment.Id.ToString()));

                if (!alreadySent)
                {
                    var auditLog = new AuditLog
                    {
                        Action = "Appointment Reminder",
                        Details = $"Automated Email/SMS Reminder Sent to Patient {appointment.Patient?.ApplicationUser?.FullName} for Appointment ID: {appointment.Id}",
                        Timestamp = DateTime.UtcNow,
                        UserId = "SYSTEM" // A system generated log
                    };
                    
                    context.AuditLogs.Add(auditLog);
                    _logger.LogInformation($"Reminder logged for Appointment {appointment.Id}");
                }
            }

            await context.SaveChangesAsync();
        }
    }
}

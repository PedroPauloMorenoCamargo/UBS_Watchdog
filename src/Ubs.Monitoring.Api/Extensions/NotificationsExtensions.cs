using Ubs.Monitoring.Application.Cases.Notifications;
using Ubs.Monitoring.Api.Notifications;

namespace Ubs.Monitoring.Api.Extensions;

public static class NotificationsExtensions
{
    public static IServiceCollection AddCaseNotifications(this IServiceCollection services)
    {
        services.AddScoped<ICaseNotificationPublisher, SignalRCaseNotificationPublisher>();
        return services;
    }
}

global using ML.Domain.Events;

namespace ML.Application.Common.Interfaces;
public interface IEmailService
{
    Task SendEmail(NotificationEvent notification, CancellationToken cancellationToken);
}

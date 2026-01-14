namespace Ubs.Monitoring.Application.Common;

public interface ICurrentRequestContext
{
    Guid? AnalystId { get; }
    string? CorrelationId { get; }
}

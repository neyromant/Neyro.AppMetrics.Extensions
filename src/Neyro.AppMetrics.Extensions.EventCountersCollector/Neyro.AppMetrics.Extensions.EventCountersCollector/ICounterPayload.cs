using App.Metrics;

namespace Neyro.AppMetrics.Extensions
{
    internal interface ICounterPayload
    {
        void Register(IMetricsRoot metrics, string eventSourceName);
    }
}

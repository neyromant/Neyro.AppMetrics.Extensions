using App.Metrics;

namespace Neyro.AppMetrics.Extensions.RuntimeStatCollector
{
    internal interface ICounterPayload
    {
        const string MetricsNamespace = "system.runtime";
        void Register(IMetricsRoot metrics);
    }
}

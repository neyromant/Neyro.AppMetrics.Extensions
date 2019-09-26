using App.Metrics;
using App.Metrics.Counter;
using System.Collections.Generic;

namespace Neyro.AppMetrics.Extensions.RuntimeStatCollector
{
    internal struct IncrementingCounterPayload : ICounterPayload
    {
        private readonly Dictionary<string, CounterOptions> _countersCache;
        public string _name;
        public double _value;
        public IncrementingCounterPayload(Dictionary<string, CounterOptions> countersCache, IDictionary<string, object> payloadFields)
        {
            _countersCache = countersCache;

            _name = payloadFields["Name"].ToString().Replace("-", "_");
            _value = (double)payloadFields["Increment"];
        }

        public void Register(IMetricsRoot metrics)
        {
            if (!_countersCache.TryGetValue(_name, out var counter))
            {
                counter = new CounterOptions { Context = ICounterPayload.MetricsNamespace, Name = _name, ResetOnReporting = true };
                _countersCache.Add(_name, counter);
            }
            var value = (long)_value;
            metrics.Measure.Counter.Increment(counter, value);
        }

    }
}

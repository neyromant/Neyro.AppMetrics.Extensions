using App.Metrics;
using App.Metrics.Counter;
using System.Collections.Generic;

namespace Neyro.AppMetrics.Extensions
{
    internal struct IncrementingCounterPayload : ICounterPayload
    {
        private readonly Dictionary<string, CounterOptions> _countersCache;
        public string _name;
        public double _value;
        public IncrementingCounterPayload(Dictionary<string, CounterOptions> countersCache, IDictionary<string, object> payloadFields)
        {
            _countersCache = countersCache;

            _name = payloadFields["Name"].ToString();
            _value = (double)payloadFields["Increment"];
        }

        public void Register(IMetricsRoot metrics, string eventSourceName)
        {
            var countersCacheKey = eventSourceName + _name;
            if (!_countersCache.TryGetValue(countersCacheKey, out var counter))
            {
                counter = new CounterOptions { Context = eventSourceName, Name = _name, ResetOnReporting = true };
                _countersCache.Add(countersCacheKey, counter);
            }
            var value = (long)_value;
            metrics.Measure.Counter.Increment(counter, value);
        }

    }
}

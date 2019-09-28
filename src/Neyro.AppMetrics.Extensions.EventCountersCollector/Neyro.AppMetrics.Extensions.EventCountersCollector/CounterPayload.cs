using App.Metrics;
using App.Metrics.Gauge;
using System.Collections.Generic;

namespace Neyro.AppMetrics.Extensions
{
    internal struct CounterPayload : ICounterPayload
    {
        private readonly Dictionary<string, GaugeOptions> _gaugesCache;
        private readonly string _name;
        private readonly double _value;

        public CounterPayload(Dictionary<string, GaugeOptions> gaugesCache, IDictionary<string, object> payloadFields)
        {
            _gaugesCache = gaugesCache;
            _name = payloadFields["Name"].ToString();
            _value = (double)payloadFields["Mean"];
        }

        public void Register(IMetricsRoot metrics, string eventSourceName)
        {
            var gaugesCacheKey = eventSourceName + _name;
            if (!_gaugesCache.TryGetValue(gaugesCacheKey, out var gauge))
            {
                gauge = new GaugeOptions { Context = eventSourceName, Name = _name };
                _gaugesCache.Add(gaugesCacheKey, gauge);
            }
            metrics.Measure.Gauge.SetValue(gauge, _value);
        }
    }
}

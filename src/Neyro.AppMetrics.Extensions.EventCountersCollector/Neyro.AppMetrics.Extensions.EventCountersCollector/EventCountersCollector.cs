using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Neyro.AppMetrics.Extensions
{
    public sealed class EventCountersCollector : EventListener, IHostedService
    {
        private readonly Dictionary<string, GaugeOptions> _gauges = new Dictionary<string, GaugeOptions>();
        private readonly Dictionary<string, CounterOptions> _counters = new Dictionary<string, CounterOptions>();
        private readonly EventCountersCollectorOptions _options;
        private readonly List<EventSource> _handledSources = new List<EventSource>();

        private readonly IMetricsRoot _metrics;

        /// <summary>
        /// Create instance of EventCountersCollector
        /// </summary>
        /// <param name="metricsRoot">Root of AppMetrics</param>
        /// <param name="options">Options for EventCountersCollector</param>
        public EventCountersCollector(IMetricsRoot metricsRoot, EventCountersCollectorOptions options)
        {
            _metrics = metricsRoot ?? throw new ArgumentNullException(nameof(metricsRoot));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            if (_options.RefreshIntervalSec < 0)
                throw new ArgumentOutOfRangeException(nameof(options.RefreshIntervalSec));
            if (_options.EnabledSources == null)
                throw new ArgumentNullException(nameof(options.EnabledSources));
            if (_options.EnabledSources.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(options.EnabledSources));

            EventSourceCreated += RuntimeEventListener_EventSourceCreated;
        }

        /// <summary>
        /// Create instance of EventCountersCollector
        /// </summary>
        /// <param name="metricsRoot">Root of AppMetrics</param>
        /// <param name="options">OptionsSnapshot for EventCountersCollector</param>
        public EventCountersCollector(IMetricsRoot metricsRoot, IOptions<EventCountersCollectorOptions> options) : this(metricsRoot, options?.Value ?? throw new ArgumentNullException(nameof(options)))
        {
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach(var source in _handledSources)
                DisableEvents(source);
            return Task.CompletedTask;
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (!"EventCounters".Equals(eventData.EventName))
                return;

            var payloadFields = eventData.Payload?[0] as IDictionary<string, object>;
            if (payloadFields == null)
                return;

            var payload = new CounterPayload(payloadFields);
            switch (payload.Type)
            {
                case CounterType.Mean:
                    RegisterGaugeValue(payload, eventData.EventSource.Name);
                    break;
                case CounterType.Increment:
                    RegisterCounterValue(payload, eventData.EventSource.Name);
                    break;
                default: 
                    break;
            }
        }

        private void RuntimeEventListener_EventSourceCreated(object? sender, EventSourceCreatedEventArgs e)
        {
            var eventSource = e.EventSource;
            if (eventSource == null)
                return;
            if (!_options.EnabledSources.Contains(eventSource.Name))
                return;

            EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All, new Dictionary<string, string?>
            {
                ["EventCounterIntervalSec"] = _options.RefreshIntervalSec.ToString()
            });

            _handledSources.Add(eventSource);
        }

        private void RegisterCounterValue(CounterPayload payload, string eventSourceName)
        {
            var countersCacheKey = eventSourceName + payload.Name;
            if (!_counters.TryGetValue(countersCacheKey, out var counter))
            {
                counter = new CounterOptions { Context = eventSourceName, Name = payload.Name, ResetOnReporting = true };
                _counters.Add(countersCacheKey, counter);
            }
            _metrics.Measure.Counter.Increment(counter, (long)payload.Value);
        }

        private void RegisterGaugeValue(CounterPayload payload, string eventSourceName)
        {
            var gaugesCacheKey = eventSourceName + payload.Name;
            if (!_gauges.TryGetValue(gaugesCacheKey, out var gauge))
            {
                gauge = new GaugeOptions { Context = eventSourceName, Name = payload.Name };
                _gauges.Add(gaugesCacheKey, gauge);
            }
            _metrics.Measure.Gauge.SetValue(gauge, payload.Value);
        }
    }
}

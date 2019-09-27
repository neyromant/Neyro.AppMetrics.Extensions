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
    public class EventCountersCollector : EventListener, IHostedService
    {
        private readonly Dictionary<string, GaugeOptions> _gauges = new Dictionary<string, GaugeOptions>();
        private readonly Dictionary<string, CounterOptions> _counters = new Dictionary<string, CounterOptions>();
        private readonly IMetricsRoot _metrics;
        private readonly EventCountersCollectorOptions _options;
        private List<EventSource> _handledSources = new List<EventSource>();


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

            ICounterPayload payload;
            if (payloadFields.ContainsKey("CounterType"))
            {
                payload = payloadFields["CounterType"].Equals("Sum")
                    ? new IncrementingCounterPayload(_counters, payloadFields)
                    : (ICounterPayload)new CounterPayload(_gauges, payloadFields);
            }
            else
            {
                payload = payloadFields.Count == 6
                    ? new IncrementingCounterPayload(_counters, payloadFields)
                    : (ICounterPayload)new CounterPayload(_gauges, payloadFields);
            }

            payload.Register(_metrics, eventData.EventSource.Name);
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
    }
}

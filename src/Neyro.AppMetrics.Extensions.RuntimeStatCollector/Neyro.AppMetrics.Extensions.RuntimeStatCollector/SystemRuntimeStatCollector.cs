using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Gauge;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;

namespace Neyro.AppMetrics.Extensions.RuntimeStatCollector
{
    public class SystemRuntimeStatCollector : EventListener, IHostedService
    {
        private readonly Dictionary<string, GaugeOptions> _gauges = new Dictionary<string, GaugeOptions>();
        private readonly Dictionary<string, CounterOptions> _counters = new Dictionary<string, CounterOptions>();
        private readonly IMetricsRoot _metrics;
        private readonly int _interval;

        private EventSource _handledSource = null;
        public SystemRuntimeStatCollector(IMetricsRoot metricsRoot, IOptions<SystemRuntimeStatCollectorOptions> options)
        {
            _metrics = metricsRoot ?? throw new ArgumentNullException(nameof(metricsRoot));
            var optionsValue = options?.Value ?? throw new ArgumentNullException(nameof(options));
            if(optionsValue.RefreshIntervalSec < 0)
                throw new ArgumentOutOfRangeException(nameof(options.Value.RefreshIntervalSec));
            _interval = optionsValue.RefreshIntervalSec;
            EventSourceCreated += RuntimeEventListener_EventSourceCreated;
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_handledSource != null)
                DisableEvents(_handledSource);
            return Task.CompletedTask;
        }

        private void RuntimeEventListener_EventSourceCreated(object sender, EventSourceCreatedEventArgs e)
        {
            var eventSource = e.EventSource;
            if (eventSource != null && eventSource.Name.Equals("System.Runtime"))
            {
                _handledSource = eventSource;
                EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All, new Dictionary<string, string>
                {
                    ["EventCounterIntervalSec"] = _interval.ToString()
                });
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (!eventData.EventName.Equals("EventCounters"))
                return;

            var payloadFields = eventData.Payload[0] as IDictionary<string, object>;

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

            payload.Register(_metrics);
        }        
    }
}

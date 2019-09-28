using App.Metrics;
using App.Metrics.Gauge;
using System.Collections.Generic;

namespace Neyro.AppMetrics.Extensions
{
    internal struct CounterPayload
    {
        public string Name { get; }
        public double Value { get; }
        public CounterType Type { get; }

        public CounterPayload(IDictionary<string, object> payloadFields)
        {
            Name = payloadFields["Name"].ToString();

            if (payloadFields.ContainsKey("CounterType"))
            {
                if (payloadFields["CounterType"].Equals("Sum"))
                {
                    Value = (double)payloadFields["Increment"];
                    Type = CounterType.Increment;
                }
                else
                {
                    Value = (double)payloadFields["Mean"];
                    Type = CounterType.Mean;
                }
            }
            else
            {
                if (payloadFields.Count == 6)
                {
                    Value = (double)payloadFields["Increment"];
                    Type = CounterType.Increment;
                }
                else
                {
                    Value = (double)payloadFields["Mean"];
                    Type = CounterType.Mean;
                }
            }
        }
    }
}

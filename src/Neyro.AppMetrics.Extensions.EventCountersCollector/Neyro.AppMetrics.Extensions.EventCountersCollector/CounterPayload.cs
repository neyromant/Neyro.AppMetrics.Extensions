using System.Collections.Generic;

namespace Neyro.AppMetrics.Extensions
{
    internal struct CounterPayload
    {
        public string Name { get; }
        public double Value { get; }
        public CounterType Type { get; }
        public IDictionary<string,string> Metadata { get; }

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

            Metadata = ExtractTagsFromMetadata(payloadFields);
        }
        

        private static Dictionary<string, string> ExtractTagsFromMetadata(IDictionary<string, object> payloadFields)
        {
            var metadata = new Dictionary<string,string>();

            if (payloadFields.ContainsKey("Metadata"))
            {
                string? payloadMetadata = payloadFields["Metadata"] as string;
                var metaKVs = payloadMetadata?.Split(",");
                if (metaKVs != null)
                    foreach(var strKv in metaKVs)
                    {
                        var kv = strKv.Split(":");
                        if (kv.Length != 2)
                            continue;
                        metadata.Add(kv[0], kv[1]);
                    }
            }

            return metadata;
        }
    }
}

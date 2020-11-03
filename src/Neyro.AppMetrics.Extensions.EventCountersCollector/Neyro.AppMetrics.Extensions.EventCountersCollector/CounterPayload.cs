using System.Collections.Generic;

namespace Neyro.AppMetrics.Extensions
{
    internal struct CounterPayload
    {
        public string Name { get; }
        public double Value { get; }
        public CounterType Type { get; }
        public IDictionary<string, string>? Metadata { get; }
        
        /// <summary>
        /// Get the key to use when creating a cache key for this counter.
        /// When metadata is used we need a separate counter for every permutation
        /// of the metadata so we use the metadata values in the key to identify
        /// the counter when caching.
        /// </summary>
        public string? Key { get; }

        public CounterPayload(IDictionary<string, object> payloadFields, bool useMetadata)
        {
            Name = payloadFields["Name"].ToString();
            Metadata = null;
            Key = Name;

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

            if (useMetadata && payloadFields.TryGetValue("Metadata", out object? metadataSource))
            {
                var payloadMetadata = metadataSource as string;
                if(string.IsNullOrWhiteSpace(payloadMetadata)) 
                    return;
                Metadata = ExtractTagsFromMetadata(payloadMetadata);
                Key = Name + payloadMetadata;
            }
        }
        
        private IDictionary<string, string>? ExtractTagsFromMetadata(string payloadMetadata)
        {
            var metaKVs = payloadMetadata.Split(",");
            var metadata = new Dictionary<string, string>(metaKVs.Length);
            foreach(var strKv in metaKVs)
            {
                var kv = strKv.Split(":");
                if (kv.Length != 2)
                    continue;
                metadata.Add(kv[0], kv[1]);
            }

            return metadata;
        }
    }
}

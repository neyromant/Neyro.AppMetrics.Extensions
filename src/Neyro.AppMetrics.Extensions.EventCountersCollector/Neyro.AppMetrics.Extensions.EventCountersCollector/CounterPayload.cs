using System.Collections.Generic;

namespace Neyro.AppMetrics.Extensions
{
    internal struct CounterPayload
    {
        public string Name { get; }
        public double Value { get; }
        public CounterType Type { get; }
        public IDictionary<string,string> Metadata { get; }
        
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
            Key = Name;
            Metadata = new Dictionary<string, string>();
            
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

            if (useMetadata)
            {
                ExtractTagsFromMetadata(payloadFields);
                // setting the key from original metadata so avoid allocation if we join
                // the values in the dictionary when creating cache key
                payloadFields.TryGetValue("Metadata", out object metadataSource);
                Key = $"{Name}{metadataSource as string}";
            }
        }



        private void ExtractTagsFromMetadata(IDictionary<string, object> payloadFields)
        {
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
                        Metadata.Add(kv[0], kv[1]);
                    }
            }
        }
    }
}

using System.Collections.Generic;

namespace Neyro.AppMetrics.Extensions
{
    /// <summary>
    /// Options for <see cref="EventCountersCollectorCollector"/>
    /// </summary>
    public class EventCountersCollectorOptions
    {
        /// <summary>
        /// Interval (sec) for update statistic
        /// </summary>
        public int RefreshIntervalSec {  get; set; } = 5;

        /// <summary>
        /// Filter for values with IntervalSec ​​less than specified. Key - EventSource name.
        /// </summary>
        public Dictionary<string, double>? IntervalSecFilter { get; set; }

        /// <summary>
        /// Whether metadata should be parsed from the event counters api and populated as AppMetrics tags
        /// Defaults to false so tags don't break existing metrics
        /// </summary>
        public bool  SetTagsFromMetadata { get; set; } = false;
        
        /// <summary>
        /// Enabled EventCounter's sources
        /// </summary>
        public string[] EnabledSources {  get; set; } = new [] { "System.Runtime" }; 
    }
}

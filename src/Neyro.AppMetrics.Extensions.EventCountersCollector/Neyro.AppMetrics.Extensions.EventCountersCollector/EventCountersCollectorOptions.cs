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
        /// Enabled EventCounter's sources
        /// </summary>
        public string[] EnabledSources {  get; set; } = new [] { "System.Runtime" }; 
    }
}

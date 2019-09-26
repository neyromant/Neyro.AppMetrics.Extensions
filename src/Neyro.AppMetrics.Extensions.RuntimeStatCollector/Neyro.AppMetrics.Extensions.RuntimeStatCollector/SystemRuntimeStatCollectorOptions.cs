namespace Neyro.AppMetrics.Extensions.RuntimeStatCollector
{
    /// <summary>
    /// Options for <see cref="SystemRuntimeStatCollector"/>
    /// </summary>
    public class SystemRuntimeStatCollectorOptions
    {
        /// <summary>
        /// Interval (sec) for update statistic
        /// </summary>
        public int RefreshIntervalSec {  get; set; } = 5; 
    }
}

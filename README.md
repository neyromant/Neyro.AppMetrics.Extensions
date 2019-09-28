# Neyro.AppMetrics.Extensions.EventCountersCollector
[AppMetrics's](https://github.com/AppMetrics/AppMetrics) extension for collect EventCounters data from EventSource's which supports it. E.g. [RuntimeEventSource](https://github.com/dotnet/coreclr/blob/release/3.0/src/System.Private.CoreLib/src/System/Diagnostics/Eventing/RuntimeEventSource.cs) or [NpgsqlEventSource](https://github.com/npgsql/npgsql/blob/dev/src/Npgsql/NpgsqlEventSource.cs)

Usage:

Just add EventCountersCollector as HostedService in your AspNetCore app.

```
services.AddHostedService(sp => new Neyro.AppMetrics.Extensions.EventCountersCollector(
	sp.GetRequiredService<IMetricsRoot>(),
    new Neyro.AppMetrics.Extensions.EventCountersCollectorOptions
    {
		RefreshIntervalSec = 5,
        EnabledSources = new[] { "System.Runtime", "Npgsql" }
    }
));
```


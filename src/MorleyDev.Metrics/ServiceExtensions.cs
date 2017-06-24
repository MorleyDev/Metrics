using Microsoft.Extensions.DependencyInjection;
using MorleyDev.Metrics.Impl;
using MorleyDev.Metrics.Options;
using System;

namespace MorleyDev.Metrics
{
	public static class ServiceExtensions
	{
		public static IServiceCollection AddTelegrafMetrics(this IServiceCollection self, Func<IServiceProvider, TelegrafConfig> getConfig)
		{
			return self.AddScoped<IMetricsService>(services => new MetricsServiceTelegrafImpl(getConfig(services)));
		}
	}
}

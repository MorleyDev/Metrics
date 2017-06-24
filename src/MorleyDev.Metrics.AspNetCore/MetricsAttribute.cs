using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace MorleyDev.Metrics.AspNetCore
{
	public class MetricsAttribute : Attribute, IResourceFilter
	{
		private readonly string _metricName;

		public MetricsAttribute(string metricName)
		{
			_metricName = metricName;
		}

		public void OnResourceExecuting(ResourceExecutingContext context)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			context.HttpContext.Items["MetricsAttribute"] = stopwatch;
		}

		public void OnResourceExecuted(ResourceExecutedContext context)
		{
			var metrics = context.HttpContext.RequestServices.GetService<IMetricsService>();
			if (metrics == null)
				return;

			var stopwatch = context.HttpContext.Items["MetricsAttribute"] as Stopwatch;
			if (stopwatch == null)
				return;

			stopwatch.Stop();

			var elapsedTotalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
			metrics.Time($"{_metricName},statuscode={context.HttpContext.Response.StatusCode}", elapsedTotalMilliseconds);
		}
	}
}

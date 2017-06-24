using System;
using System.Collections.Generic;

namespace MorleyDev.Metrics
{
	public interface IMetricsService
	{
		void Emit(string key, IDictionary<string, int> values);
		void Time(string key, double milliseconds);
		void Time(string key, double milliseconds, IDictionary<string, int> extras);
		void Count(string key, string name, int amount = 1);
		IDisposable Timed(string name);
	}
}

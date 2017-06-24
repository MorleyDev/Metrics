using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MorleyDev.Metrics.Options;

namespace MorleyDev.Metrics.Impl
{
	public class MetricsServiceTelegrafImpl : IMetricsService
	{
		private readonly TelegrafConfig _config;

		public MetricsServiceTelegrafImpl(TelegrafConfig config)
		{
			_config = config;
		}

		public void Time(string key, double milliseconds)
		{
			var timeStamp = GetTimeStamp();

			Task.Run(() => SendDatagram($"{key},app={_config.App} ms={milliseconds} {timeStamp}"));
		}

		public void Time(string key, double milliseconds, IDictionary<string, int> extras)
		{
			var timeStamp = GetTimeStamp();

			Task.Run(() =>
			{
				var values = string.Join(" ", extras.Select(k => $"{k.Key}={k.Value}i").Concat(new[] { $"ms={milliseconds}" }).ToArray());

				return SendDatagram($"{key},app={_config.App} {values} {timeStamp}");
			});
		}

		public void Count(string key, string name, int amount = 1)
		{
			var timeStamp = GetTimeStamp();

			Task.Run(() => SendDatagram($"{key},app={_config.App} {name}={amount}i {timeStamp}"));
		}

		public void Emit(string key, IDictionary<string, int> values)
		{
			var timeStamp = GetTimeStamp();
			var valueSet = string.Join(" ", values.Select(k => $"{k.Key}={k.Value}i").ToArray());

			Task.Run(() => SendDatagram($"{key},app={_config.App} {valueSet} {timeStamp}"));
		}

		private static long GetTimeStamp()
		{
			return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000000;
		}

		private Task SendDatagram(string datagram)
		{
			return SendDatagram(Encoding.UTF8.GetBytes(datagram));
		}

		private async Task SendDatagram(byte[] datagram)
		{
			using (var client = new UdpClient())
			{
				await client.SendAsync(datagram, datagram.Length, _config.Hostname, _config.Port);
			}
		}

		public IDisposable Timed(string name)
			=> _config.Enabled ? MakeMetricsTimer(name) : new NullDisposable();

		private IDisposable MakeMetricsTimer(string name) => new MetricsTimer(name, _config.App, this);

		private class MetricsTimer : IDisposable
		{
			private readonly string _name;
			private readonly string _app;
			private readonly MetricsServiceTelegrafImpl _factory;
			private readonly Stopwatch _stopwatch;

			public MetricsTimer(string name, string app, MetricsServiceTelegrafImpl factory)
			{
				_name = name;
				_app = app;
				_factory = factory;

				_stopwatch = new Stopwatch();
				_stopwatch.Start();
			}

			public void Dispose()
			{
				_stopwatch.Stop();

				var time = _stopwatch.Elapsed.TotalMilliseconds;
				var timeStamp = GetTimeStamp();

				Task.Run(() => _factory.SendDatagram($"{_name},app={_app} ms={time} {timeStamp}"));
			}
		}

		private class NullDisposable : IDisposable { public void Dispose() { } }
	}
}

namespace MorleyDev.Metrics.Options
{
	public class TelegrafConfig
	{
		public bool Enabled { get; set; } = true;

		public string Hostname { get; set; } = "localhost";

		public string App { get; set; } = "unknown";

		public int Port { get; set; } = 8092;
	}
}
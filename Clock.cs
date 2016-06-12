using System;
using System.Diagnostics;

namespace TinyMUD
{
	public static class Clock
	{
		public static readonly DateTime UTC = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		private static readonly Stopwatch sinceStartup = new Stopwatch();
		private static double timeStartup;

		public static long Now
		{
			get { return (long)((timeStartup + sinceStartup.Elapsed.TotalMilliseconds) * 1000 + 0.5); }
		}

		public static long Elapsed
		{
			get { return (long)(sinceStartup.Elapsed.TotalMilliseconds * 1000 + 0.5); }
		}

		public static void Initialize()
		{
			if (!sinceStartup.IsRunning)
			{
				sinceStartup.Start();
				TimeSpan ts = DateTime.UtcNow - UTC;
				timeStartup = ts.TotalMilliseconds;
			}
		}
	}
}
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AlertConsoleCore
{
	static class Config
	{
		public static IConfigurationBuilder builder = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appSettings.json");

		public static IConfigurationRoot configurationRoot = builder.Build();
		public static IConfigurationSection signalRApiUrl = configurationRoot.GetSection("signalRApiUrl");
	}
}

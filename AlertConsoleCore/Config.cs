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
		public static IConfigurationSection mqttClientID = configurationRoot.GetSection("mqttClientID");
		public static IConfigurationSection mqttUser = configurationRoot.GetSection("mqttUser");
		public static IConfigurationSection mqttPassword = configurationRoot.GetSection("mqttPassword");
		public static IConfigurationSection mqttCleanSession = configurationRoot.GetSection("mqttCleanSession");
		public static IConfigurationSection mqttIp = configurationRoot.GetSection("mqttIp");
	}
}

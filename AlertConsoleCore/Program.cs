using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebAPISignalR.Models;

namespace AlertConsoleCore
{
	class Program
	{
		static HttpClient client = new HttpClient();

		static void Main(string[] args)
		{
			Console.WriteLine("主控台開始...");

			string message = "警告訊息：" + DateTime.Now.ToShortTimeString();
			var url = NotifyAlertMessageList(message);
			Console.WriteLine($"NotifyAlertMessageList url: {url},Message:{message}");

			Console.ReadLine();
		}

		/// <summary>
		/// 通知警告訊息並列出列表
		/// </summary>
		/// <param name="Message"></param>
		/// <returns></returns>
		private static async Task<Uri> NotifyAlertMessageList(string Message)
		{
			client.BaseAddress = new Uri(Config.signalRApiUrl.Value);
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			try
			{
				MessageModel msgModel = new MessageModel
				{
					Message = Message
				};

				HttpResponseMessage response = await client.PostAsJsonAsync(
					"api/Broadcast/Get", msgModel);
				response.EnsureSuccessStatusCode();

				HttpResponseMessage response2 = await client.PostAsJsonAsync(
					"api/Broadcast/ShowSingleMessage", msgModel);
				response2.EnsureSuccessStatusCode();

				return response.Headers.Location;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}

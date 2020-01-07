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

			//TODO:自行改寫要傳給 message 的值
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
			//設定API網址
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

				//第一支API
				HttpResponseMessage response = await client.PostAsJsonAsync(
					"api/Broadcast/Get", msgModel);
				response.EnsureSuccessStatusCode();

				//第二支API
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

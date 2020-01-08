using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ViewModel;

namespace AlertConsoleCore
{
	class Program
	{
		#region 建立一個新的MQTT客戶端
		static MqttFactory factory = new MqttFactory();
		static IMqttClient mqttClient = factory.CreateMqttClient();
		static readonly MqttClientOptions clientOptions = new MqttClientOptions
		{
			ClientId = Config.mqttClientID.Value,
			Credentials = new MqttClientCredentials()
			{
				Username = Config.mqttUser.Value,
				Password = Encoding.UTF8.GetBytes(Config.mqttPassword.Value)
			},
			CleanSession = Convert.ToBoolean(Config.mqttCleanSession.Value),
			ChannelOptions = new MqttClientTcpOptions
			{
				Server = Config.mqttIp.Value
			}
		};
		#endregion

		static async Task Main(string[] args)
		{
			Console.WriteLine("主控台開始...");

			//簡單範例:請自行改寫要傳給 message 的值
			/*
			string message = "警告訊息：" + DateTime.Now.ToShortTimeString();
			var url = NotifyAlertMessageList(message);
			Console.WriteLine($"NotifyAlertMessageList url: {url},Message:{message}");
			*/

			await InitMQTT();

			Console.ReadLine();
		}

		/// <summary>
		/// 初始化MQTT
		/// </summary>
		private static async Task InitMQTT()
		{
			try
			{
				mqttClient.UseConnectedHandler(async e =>
				{
					Console.WriteLine("### 連接MQTT Server ###");

					// Subscribe to a topic
					await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("home/send").Build());

					Console.WriteLine("### SUBSCRIBED ###");
				});

				mqttClient.UseDisconnectedHandler(async e =>
				{
					Console.WriteLine("### 從MQTT Server中斷連線 ###");
					await Task.Delay(TimeSpan.FromSeconds(1));
				});

				await mqttClient.ConnectAsync(clientOptions);

				// Setup ApplicationMessageReceived handler
				mqttClient.UseApplicationMessageReceivedHandler(e =>
				{
					Console.WriteLine("### 接收訊息 ###");
					Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");

					var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
					var result = (MachineStatus)JsonConvert.DeserializeObject(payload, typeof(MachineStatus));
					Console.WriteLine($"+ Payload = {payload}");

					string msg = string.Empty;
					if (result.TurnOn)
						msg = "機器已開啟！";
					else
						msg = "機器關機了！";

					var url =  NotifyAlertMessageList(msg);

					Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
					Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
					Console.WriteLine();
				});

				ConnectMQTT(clientOptions);
				Thread checkThread = new Thread(new ThreadStart(CheckMqttConnect));
				checkThread.Start();
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// 檢查MQTT連線
		/// </summary>
		private static void CheckMqttConnect()
		{
			while (true)
			{
				try
				{
					if (mqttClient.IsConnected == false)
					{
						Console.WriteLine("與MQTT連線斷開");
						Console.WriteLine("嘗試重新連線...");
						ConnectMQTT(clientOptions);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(JsonConvert.SerializeObject(e));
				}
				Thread.Sleep(10000);
			}
		}

		/// <summary>
		/// 連接MQTT
		/// </summary>
		/// <param name="clientOptions"></param>
		private static void ConnectMQTT(MqttClientOptions clientOptions)
		{
			try
			{
				while (mqttClient.IsConnected == false)
				{
					try
					{
						mqttClient.ConnectAsync(clientOptions);
					}
					catch (Exception ex)
					{
						Console.WriteLine("### 連接失敗 ###" + Environment.NewLine + ex);
						Console.WriteLine("1秒後嘗試重新連線...");
					}
					Thread.Sleep(1000);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(JsonConvert.SerializeObject(e));
			}
		}

		/// <summary>
		/// 通知警告訊息並列出列表
		/// </summary>
		/// <param name="Message"></param>
		/// <returns></returns>
		private static async Task<Uri> NotifyAlertMessageList(string Message)
		{
			//設定API網址
			HttpClient httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri(Config.signalRApiUrl.Value);
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			try
			{
				MessageModel msgModel = new MessageModel
				{
					Message = Message
				};

				//第一支API
				HttpResponseMessage response = await httpClient.PostAsJsonAsync(
					"api/Broadcast/Get", msgModel);
				response.EnsureSuccessStatusCode();

				//第二支API
				HttpResponseMessage response2 = await httpClient.PostAsJsonAsync(
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

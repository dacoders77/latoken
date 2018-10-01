using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using WebSocketSharp;
using Newtonsoft.Json.Linq;

namespace HitBtc
{
	public partial class Form1 : Form
	{
		public WebSocket ws;
		DateTime WebScocketLastMessage = new DateTime();

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			// Websocket class instance
			ws = new WebSocket("wss://api.hitbtc.com/api/2/ws"); // wss://www.bitmex.com/realtime wss://testnet.bitmex.com/realtime

			InitializeWebSocket(); // WebSocket warm-up

		}

		private void InitializeWebSocket()
		{


			ws.OnMessage += (sender, e) =>
			{
				WebScocketLastMessage = DateTime.UtcNow;
				try
				{
					JObject message = JObject.Parse(e.Data);
					//MessageBox.Show("Form1.cs ws.OnMessage line 47 " + Message.ToString());
					Console.WriteLine("Form1.cs" + message);
				}
				catch
				{
					MessageBox.Show("Form1.cs line 49. Exception");
				}
			};

			ws.OnError += (sender, e) =>
			{
				MessageBox.Show("Form1.cs line 58. ws.OnError" + e);
			};

			ws.Connect(); // Open WS connection

			// Convert json object to escaped string
			// http://www.tools.knowledgewalls.com/jsontostring

			// Auth
			//ws.Send("{\"method\": \"login\",\"params\": {\"algo\": \"BASIC\",\"pKey\": \"b03ad090c697ccda1301****\",\"sKey\": \"a290c120b3239cb3547df78ab1****\"}}");
			ws.Send("{\"method\": \"login\",\"params\": {\"algo\": \"BASIC\",\"pKey\": \"" + Settings.pKey + "\",\"sKey\": \"" + Settings.sKey + "\"}}");

			// Subscribe to reports (Order statuses)
			//ws.Send("{\"method\": \"subscribeTicker\", \"params\": {\"symbol\": \"ETHBTC\"},\"id\": 123 } "); // Works good

			// New order
			Int32 b = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
			ws.Send("{\"method\": \"newOrder\",\"params\": {\"clientOrderId\": \"" + b + "\",\"symbol\": \"EOSUSD\",\"side\": \"buy\",\"type\": \"market\",\"price\": \"0.059837\",\"quantity\": \"0.01\"},\"id\": 123}");

			// Subscribe to ticker
			//ws.Send("{\"method\": \"subscribeTicker\", \"params\": {\"symbol\": \"ETHBTC\"},\"id\": 123 } "); // Works good




		}
	}
}

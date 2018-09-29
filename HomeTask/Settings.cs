using System;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json.Schema;

namespace BitMEXAssistant

/* Settings file. Sensetive settings like keys, password etc. are located in a separate file.
 * DB host is different on the local and production server.
 * hometask_settings.json is added to gitignor.
 */

{
	public static class Settings
		{
			//
			public static string pKey { get; }
			public static string sKey { get; }
			public static string dbHost { get; }
			public static string bitmexApiKey { get; }
			public static string bitmexApiSecret { get; }
			public static string bitmexDemoApiKey { get; }
			public static string bitmexDemoApiSecret { get; }
			public static string hitbtcApiKey { get; }
			public static string hitbtcApiSecret { get; }

		static Settings()
			{
				try
				{
					string jsonFromFile = File.ReadAllText("hometask_settings.json");
					JSchema jsonParsed = JSchema.Parse(jsonFromFile);
					var x = Newtonsoft.Json.Linq.JObject.Parse(jsonFromFile);

					pKey = (string)x["pKey"];
					sKey = (string)x["sKey"]; 
					dbHost = (string)x["dbHost"];
					bitmexApiKey = (string)x["bitmexApiKey"];
					bitmexApiSecret = (string)x["bitmexApiSecret"];
					bitmexDemoApiKey = (string)x["bitmexDemoApiKey"];
					bitmexDemoApiSecret = (string)x["bitmexDemoApiSecret"];
					hitbtcApiKey = (string)x["hitbtcApiKey"];
					hitbtcApiSecret = (string)x["hitbtcApiSecret"];
				}
				catch (Exception exception)
				{
					MessageBox.Show("Settings.cs. Error opening Setting.json: " + exception.Message);
				}
			}
		}
}

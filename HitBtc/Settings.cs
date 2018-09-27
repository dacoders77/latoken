using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json.Schema;

namespace HitBtc
{
	public static class Settings
	{
		//
		public static string pKey { get; }
		public static string sKey { get; }
		public static string dbHost { get; }

		static Settings()
		{
			try
			{
				string jsonFromFile = File.ReadAllText("tfr_settings.json"); // File must be located in Debug folder or next whre .exe is placed

				JSchema jsonParsed = JSchema.Parse(jsonFromFile);
				var x = Newtonsoft.Json.Linq.JObject.Parse(jsonFromFile);

				pKey = (string)x["pKey"];  
				sKey = (string)x["sKey"];

			}
			catch (Exception exception)
			{
				MessageBox.Show("Settings.cs. Error opening Setting.json: " + exception.Message);
			}
		}
	}
}

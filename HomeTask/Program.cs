using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitMEX;

namespace BitMEXAssistant
{
	static class Program
	{
	    private static MainController _mainController;
	    private static Form1 _mainForm;
	    /// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

            _mainController = new MainController(
                new BitmexRealtimeDataService(
                    new BitmexDataService()
                ),
                _mainForm = new Form1());
            
			Application.Run(_mainForm);
		}
	}
}

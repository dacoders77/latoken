
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing; // что бы к точкам Point обращаться.
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using System.Reflection; // установление флагов для панелей, что бы не моргало

namespace BitMEXAssistant
{

	public class DomSymbol1
	{

		// графический вид стакана
		public int points_graph_count = 30; //38 // количество точек по которым строим график
		public int points_graph_step = 15; // шаг точек графика
		public int j = 460; // положение по Х, горизонтальная координата. расстояние от левого края панели до стакана. ширина поля на котором рисуется график
		public Font stakan_font = new Font("Calibri", 10); // шрифт в строчках стакана
		public int interval_mezh_strok = 4; // интервал между строк
		int suzhenie_fona = 0; // насколько будет сужен прямоугольник используемый в качестве фона. отрицательные значение (-5) вызовет расширение фона
		bool show_ramka = false; // отображать рамку
		int dlina_fona_zayav = 54; // длина поля стакана, выделяемое под заявки. данная переменная указывается руками далее вычисляется длина строки (dlina_fona_stroki) ибо неизвестно сколько знаков в цене инструмента, которым торгуем. потом данные переменные складываются и получается ширина стакана.
		int dlina_fona_stroki; // длина фона прямоугольника под строку стакана. заяка + цена

		// ширина столбика заявки. чем больше стоит заявки - тем больше стобик. рассчитывается, как квадратный корень из объема. ширина стакана 100 пикселей. соответственно корень квадратный из 10.000 - 100. 
		// получается, что столбик нормально может отображать до 10.000 заявок. что бы охватить больший объем - можно попробовать брать кубический корень. сейчас, если вылезет за 10.000 - ограничиваем столбик и выводим его красным цветом.
		int stolbik_zayav_length_ask;
		int stolbik_zayav_length_bid;

		// цвета столбиков в зависимости от объема. цвета вычисляются в зависимости от объема. максимальный объем, который может отобразить столбик зависит от его ширины, которую можно задать в переменной dlina_fona_zayav. сейчас столбик = 100 пикселей. соответственно может влезть sqrt(10000) = 100;
		Color stolbik_color_ask;
		Color stolbik_color_bid;


		// вид кружочков и пороги объемом для вычисления размера кружочков
		Font kroog_font = new Font("Calibri", 10, FontStyle.Bold); // определим шрифт для кружочков
		int limit_size_1 = 10; // пороги вывода кружочков на сделках (объем). 3 варианта: 1ый < 1ый порог. 2ой: > 1ого < ого. 3ий: > 2ого. когда прошла сделка с объемом < 1ого порога - кружочик выводится в виде точки
		int limit_size_2 = 100;
		int kroog_diametr_1 = 6; // 3 вида кругов для соответствующих объемом
		int kroog_diametr_2 = 16;
		int kroog_diametr_3 = 24;



		volatile bool running; // флаг отрисовки панели
		public string[] strArray; // массив всех цен инструмента, которые будут выводится на панель. - цена от планки до планки
		object objLocker; // тип объект. может принимать любой тип для хранения любой переменной. нужна для локера

		public Thread panel_potok; // поток

		Form1 form1_root; // объявим переменную в которой будем хранить ссылку form на основной класс.
		public int price_end; // high limit верх цены инструмента
		public int price_start; // low limit низ цены
		public int price_step = 10; // шаг цены инструмента
		int i = 10; // индекс для обращения к массиву всех цен инструмента, которым торгуем. от планки до планки.



		// массивы
		public List<Point> line_points_ask = new List<Point>(); // коллекция для хранения точек для построения линии ASK. заполняеится из update_bid_ask
		public List<Point> line_points_bid = new List<Point>(); // // коллекция для хранения точек для построения линии BID. заполняеится из update_bid_ask
		public List<Point> tick_points = new List<Point>(); // для хранения тиков. заполняем данный массив из add_tick
		public List<Point> market_delta_mass = new List<Point>(); // для хранения точек графика маркет дельты
		public List<Point> market_delta_stop_loss = new List<Point>(); // для хранения графика стоп лосса маркет дельты. того графика, который идет за подтягивающимся кружочком

		// массивы и переменные для кружочков
		public List<double> volume_mass = new List<double>(); // для хранения объемов. параллельно массиву line_points делаем еще один, в котором, под такимиже индексами храним объем. нужен для того, что бы при привышении определенного объема, рисовать круги разнго размера
		public List<string> direction_mass = new List<string>(); // для хранения направления сделок. buy или sell. параллельно volume_mass и line_points. нужен для определения цвета кругжочков.
		bool deal_direction = true; // флаг сдлелки. тру - сделка бай. нужен для вывода кружочко разного цвета при выводе на экран и переборе циклом всего массива объемом
		int kroog_size; // размер круга. рассчитывается на ходу в зависимости от направления и объема сделки
		bool show_string = false; // флаг показывать или нет цифру в кружочке

		// двумерный массив под стакан
		public int[,] stakan_mass = new int[50, 4]; // матрица хранения всего стакана. заполняется из update_bid_ask.размерность 50x4. одна строчка стодержит и бид и аск стакана каждой строки + объемы
													// каждая строчка матрицы содержит соответствующую строчку стакана. элементы 0ый: объем аск, 1ый: цена аск, 2ой: объем бид, 3ий: цена бид. элементы вертикально идет под индексами от 0 - 49.
													// таким образом для обращения к нужной нам цене бида, аска или объема - мы просто обращаемся к элементу матрицы под соответствующими индексами.

		bool promotka = true; // флаг для прокрутки стакана к той цене, которая торгуется. выполняется один раз


		// высота пустого места от верхнего края tab_control до ряда list_view. там будут располагаться вкладки.
		int vkladhi_heidht = 30;

		// панели для стаканов, лист вью, подписи и вкладки
		public Panel panel_pod_stakan; // панель для стакана. внутри нее будет еще одна, а на ней уже - сам стакан.
		public Panel panel_stakan; // на этой панели располагается стакан
		public Panel panel_pod_dlt; // панели под маркет дельту
		public Panel panel_dlt; // на этой панели располагается индикатор маркет дельты

		public Panel panel_tape; // панель, на нее будем выводить все сделки ибо list_view и list_box тормозят систему

		public ListBox list_box_tape; /* лист бокс тестовый для выводу туда сделок ибо лист вью тормозит */
		public ListView list_view_filter1; // filter 1
		public ListView list_view_filter2; // filter 2
		public ListView list_view_system;
		public ListView list_view_updateorder;
		public ListView list_view_addtrade;
		public ListView list_view_trade;

		public Panel panel_2; // панель справа, на ней будут все окна логов
		public Panel panel_tape_podpis; // подпись для подписи tape
		public Panel panel_filter1_podpis;
		public Panel panel_filter2_podpis;
		public Panel panel_system_podpis;
		public Panel panel_updateorder_podpis;
		public Panel panel_addtrade_podpis;
		public Panel panel_trade_podpis;

		public Panel panel_vkladka_1; // панелья для первой вкладки
		public Panel panel_vkladka_2;
		public Panel panel_vkladka_3;



		public Font podpis_font = new Font("Calibri", 13); // шрифт подписей окон, в правом верхнем углу

		public string tape_podpis_text = "tape"; // тексты подписей для окон 
		public string filter1_podpis_text = "filter_1";
		public string filter2_podpis_text = "filter_2";

		public string system_podpis_text = "system";
		public string updateorder_podpis_text = "update_order";
		public string addtrade_podpis_text = "add_trade";
		public string trade_podpis_text = "trade";

		public string panel_vkladka_1_text = "main"; // тексты подписей эмитаторов вкладок
		public string panel_vkladka_2_text = "slave";
		public string panel_vkladka_3_text = "graph";

		// метки статуса соединения
		public Label label_name;
		public Label label_connect_status;
		public Label label_connect;

		// кнопки коннект диссконект
		public Button connect_button;
		public Button disconnect_button;





		public DomSymbol1(Form1 form) // Конструктор класса
		{

			objLocker = new object();

			//form.panel_market_delta.Paint += panel_market_delta_paint; // подцепили событие отрисовки панели маркет дельта

			form1_root = form; // положим form в form_root - нужно это для того, что бы иметь доступ к form через переменную from1_root, в которой находится ссылка this основной формы

			if (running) // если по какой то причине поток запущен - выскакиваем из цикла
				return;

			running = true;
			panel_potok = new Thread(new ThreadStart(ThreadToDo)); // создали экземпляр потока
			panel_potok.IsBackground = true;
			panel_potok.Name = "class_stakan_thread";
			panel_potok.Start(); // запустили поток


		} // class_stakan


		public void panel_small_paint(object sender, PaintEventArgs e) // событие паинт малой панели, на которой расположен стакан
		{

			if (strArray == null) // если массив пуст - выскакиваем из метода
			{
				return;
			}


			Graphics g = e.Graphics; // создали графику
			g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество


			// рисуем цены инструмента в столбик на панели panel_small
			//lock (objLocker) // lock нужен что бы не моргали выводимые цифры на панели. оказывается и так работает
			//{
			var pt = new Point(j, 0); // отступы от верхнего левого края. x,y x - по горизонтали, y - по вертикали
			for (int i = 0; i < strArray.Length; ++i) // проходим по всему массиву. массив на этом моменте уже заполнен
			{
				g.DrawString(strArray[i], stakan_font, Brushes.Black, pt); // отрисовываем строку. 10ый шрифт (высота 15 пикс), проставляется в мастере
				pt.Y += (int)stakan_font.GetHeight() + interval_mezh_strok; // смещение по координам вертикально с шагом 3. 3 это расстояние между строчками, от края, в не от середины
			}

			//} // lock



			dlina_fona_stroki = dlina_fona_zayav + (int)g.MeasureString(stakan_mass[0, 1].ToString(), stakan_font).Width; // подсчитаем длину строки. залезем в первую строчку матрицы бид и аска и возьмем оттуда значение. смысл этого в том, что мы не знаем скоько знаков в цене инструмента. для это и лезем

			for (int x = 0; x < stakan_mass.Length / 4; x++)// цикл по всем элементам массива stakan_mass. stakan_mass.Length / 4 - количество столбцов, так как массив двумерный-квадратный
			{
				#region рассчитываем ширину и цвет столбика, который отображает объем заявок в цене
				// ask
				if (Math.Sqrt(stakan_mass[x, 0]) < dlina_fona_stroki / 2) // проверяем объем ask, который вылезает за половину рассчитанной ширины стакана
				{
					stolbik_zayav_length_ask = (int)Math.Sqrt(stakan_mass[x, 0]);
					stolbik_color_ask = Color.Khaki;
				}

				if (Math.Sqrt(stakan_mass[x, 0]) > dlina_fona_stroki / 2 && Math.Sqrt(stakan_mass[x, 0]) < dlina_fona_stroki) // если объем больше половины ширины
				{
					stolbik_zayav_length_ask = dlina_fona_stroki;
					stolbik_color_ask = Color.Tomato;
				}

				if (Math.Sqrt(stakan_mass[x, 0]) > dlina_fona_stroki) // если объем(столбик) вылезает за границы
				{
					stolbik_zayav_length_ask = dlina_fona_stroki;
					stolbik_color_ask = Color.Red;
				}



				// bid
				if (Math.Sqrt(stakan_mass[x, 2]) < dlina_fona_stroki / 2)
				{
					stolbik_zayav_length_bid = (int)Math.Sqrt(stakan_mass[x, 2]);
					stolbik_color_bid = Color.Khaki;
				}

				if (Math.Sqrt(stakan_mass[x, 2]) > dlina_fona_stroki / 2 && (Math.Sqrt(stakan_mass[x, 2]) < dlina_fona_stroki))
				{
					stolbik_zayav_length_bid = (int)Math.Sqrt(stakan_mass[x, 2]);
					stolbik_color_bid = Color.Tomato;
				}

				if (Math.Sqrt(stakan_mass[x, 2]) > dlina_fona_stroki)  // если столбик вылезает за границы
				{
					stolbik_zayav_length_bid = dlina_fona_stroki;
					stolbik_color_bid = Color.Red;
				}
				#endregion




				//ASK фон                                                      x  y                                                                         номер строчки                             -y  width  -width  hight                                                     
				g.FillRectangle(new SolidBrush(Color.LightPink), new Rectangle(j, ((((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - stakan_mass[x, 1]) / price_step) + suzhenie_fona / 2), dlina_fona_stroki, (int)stakan_font.GetHeight() - suzhenie_fona)); // общий фон строки
				g.FillRectangle(new SolidBrush(stolbik_color_ask), new Rectangle(j, ((((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - stakan_mass[x, 1]) / price_step) + suzhenie_fona / 2), stolbik_zayav_length_ask, (int)stakan_font.GetHeight() - suzhenie_fona)); // полосочка под заявки. меняет цвет в зависимости от объема.

				// цена ячейки + объем ask
				g.DrawString(stakan_mass[x, 0].ToString(), stakan_font, Brushes.Black, new Point(j, ((((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - stakan_mass[x, 1]) / price_step)))); // выведем объем
				g.DrawString(stakan_mass[x, 1].ToString(), stakan_font, Brushes.Black, new Point(j + dlina_fona_zayav, ((((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - stakan_mass[x, 1]) / price_step)))); // выведем цену



				//BID фон
				g.FillRectangle(new SolidBrush(Color.LimeGreen), new Rectangle(j, ((((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - stakan_mass[x, 3]) / price_step) + suzhenie_fona / 2), dlina_fona_stroki, (int)stakan_font.GetHeight() - suzhenie_fona)); // общий фон строки
				g.FillRectangle(new SolidBrush(stolbik_color_bid), new Rectangle(j, ((((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - stakan_mass[x, 3]) / price_step) + suzhenie_fona / 2), stolbik_zayav_length_bid, (int)stakan_font.GetHeight() - suzhenie_fona)); // полосочка под заявки. меняет цвет в зависимости от объема.

				// цена ячейки + объем bid
				g.DrawString(stakan_mass[x, 2].ToString(), stakan_font, Brushes.Black, new Point(j, ((((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - stakan_mass[x, 3]) / price_step))));
				g.DrawString(stakan_mass[x, 3].ToString(), stakan_font, Brushes.Black, new Point(j + dlina_fona_zayav, ((((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - stakan_mass[x, 3]) / price_step))));



				// рамка                                                                                                                                                                   width                                                                                                                                height
				if (show_ramka) // отображать рамку
				{
					g.DrawRectangle(new Pen(Color.Black, 1), j, ((((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - stakan_mass[x, 1]) / price_step) + suzhenie_fona / 2), ((int)g.MeasureString(stakan_mass[x, 0].ToString() + "  " + stakan_mass[x, 1].ToString(), stakan_font).Width + interval_mezh_strok), ((int)stakan_font.GetHeight() - suzhenie_fona));
					g.DrawRectangle(new Pen(Color.Black, 1), j, ((((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - stakan_mass[x, 3]) / price_step) + suzhenie_fona / 2), ((int)g.MeasureString(stakan_mass[x, 2].ToString() + "  " + stakan_mass[x, 1].ToString(), stakan_font).Width + interval_mezh_strok), ((int)stakan_font.GetHeight() - suzhenie_fona));
				}

				//g.Dispose(); // утилизируем графику

			} // for



			#region //рисуем график аска. конвертируем коллекцию в массив ибо драв лайнс не принимает коллекцию на входе

			Point[] points_arry_ask = line_points_ask.ToArray();
			if (line_points_ask.Count > 1) // рисуем линю только при наличии 2ух или боле точек. иначе вылезает ошибка
										   //g.DrawLines(new Pen(Color.FromArgb(100, 255, 0, 0), 2), points_arry_ask);
				g.DrawLines(new Pen(Color.Red, 1), points_arry_ask);

			// bid
			Point[] points_arry_bid = line_points_bid.ToArray();
			if (line_points_ask.Count > 1) // рисуем линю только при наличии 2ух или боле точек. иначе вылезает ошибка
										   //g.DrawLines(new Pen(Color.FromArgb(100, 0, 255, 0), 2), points_arry_bid);
				g.DrawLines(new Pen(Color.Green, 1), points_arry_bid);

			// рисуем график тиков (сделок)
			Point[] tick_arry = tick_points.ToArray();
			if (tick_points.Count > 1)
				g.DrawLines(new Pen(Color.FromArgb(200, 0, 0, 0), 3), tick_arry);
			#endregion


			#region // рисуем кружочки

			for (int z = 0; z < tick_points.Count; z++)
			{
				// определение направления сделки в зависимости от элемента массива
				if (direction_mass[z] == "buy") deal_direction = true; else deal_direction = false;


				// определение размера кружочка в зависимости от объема
				if (volume_mass[z] <= limit_size_1) { kroog_size = kroog_diametr_1; }; // <= 10
				if (volume_mass[z] > limit_size_1 && volume_mass[z] < limit_size_2) { kroog_size = kroog_diametr_2; show_string = true; }; // > 10 < 100
				if (volume_mass[z] >= limit_size_2) { kroog_size = kroog_diametr_3; show_string = true; }; // >=100


				// в соответствии с данными из массивов - рисуем круг
				if (deal_direction) g.DrawEllipse(new Pen(Color.Green, 1), tick_points[z].X - kroog_size / 2, tick_points[z].Y - kroog_size / 2, kroog_size, kroog_size);
				else g.DrawEllipse(new Pen(Color.LightCoral, 1), tick_points[z].X - 3, tick_points[z].Y - 3, 6, 6); // Х - горизонтальная координата, Y - вертикальная. элипс рисуется от верхнего правого круга по этому от места точки - нужно отступить 1/2 диавметра влево и вверх. 6 - диаметр, 3 - радиус

				try
				{
					// рисуем фон круга
					if (deal_direction) g.FillEllipse(Brushes.LimeGreen, tick_points[z].X - kroog_size / 2, tick_points[z].Y - kroog_size / 2, kroog_size, kroog_size);
					else g.FillEllipse(Brushes.LightPink, tick_points[z].X - kroog_size / 2, tick_points[z].Y - kroog_size / 2, kroog_size, kroog_size);
				}
				catch (Exception err)
				{
					//mail.send("post@websms.ru", "user=nasled pass=658200 fromPhone=robot-hobot tels=+79108925002 mess=ошибка в tick_points[z] отловлена!", "", false, "C:\\1.txt"); // кому, тело письма, заголовок, путь к вложению C:\\bd.rar
					//form1_root.logging.log_add(form1_root, "filter1", "tick_points[z] ", "" + err, 4);

					MessageBox.Show("tick_points[z] " + err);
				}

				//ошибка вылезает индекс за пределами диапазона? есть подозрение, что в момент очистки очереди происходит обращеие в пустой массив

				// цифра в кружочке
				if (show_string) g.DrawString(volume_mass[z].ToString(), kroog_font, Brushes.Black, (tick_points[z].X - g.MeasureString(volume_mass[z].ToString(), kroog_font).Width / 2), tick_points[z].Y - kroog_font.Height / 2); // // выводим объем текстом. что бы определить куда нужно выводить строку - нужно ее длину поделить пополам и результат вычесть из точки. длина строки зависит от того, каким шрифтом пишем

				show_string = false; // переведем флаг показывания цифры в кружочке в фолс, что бы если придет сдлека с объемом < 10 не выводить надпись

			} // for


			#endregion








			// скролл панелей до видимой зоны. и стакана и маркет дельты
			if (promotka && stakan_mass[0, 3] != 0) // промотаем стакан к торгуемой цене. проматываем только тогда, когда пришли планци цены. когда появились цены в матрице стакана - значит и планки пришли. проверяем цену 0-вой строчки на неравенство 0
			{
				form1_root.stakan.panel_pod_stakan.AutoScrollPosition = new Point(0, ((((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - stakan_mass[0, 3]) / price_step)) - 400); // рассчитали координату и отняли примерно половину высоты экрана
				promotka = false;
			}



			//g.DrawString(ask_0.ToString(), stakan_font, Brushes.Red, new Point(90, 20));
			//g.FillRectangle(new SolidBrush(Color.LightPink), new Rectangle(90, 20, 50, 20)); // работает

			//Pen pen = new Pen(Color.Purple); // работает
			//g.DrawRectangle(pen, 90, 20, 50, 20);




		} // panel_small_paint



		/* // панель маркет дельты
        public void panel_market_delta_paint(object sender, PaintEventArgs e) // событие паинт панели маркет дельта
        {
            Graphics g = e.Graphics; // создали графику
            g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество

            g.FillRectangle(new SolidBrush(Color.Red), new Rectangle(j, (int)form1_root.market_delta_vol, 50, 20)); // квадратик маркет дельты
            g.DrawString((form1_root.panel_market_delta.Height / 2 - form1_root.market_delta_vol).ToString(), stakan_font, Brushes.White, new Point(j, (int)form1_root.market_delta_vol)); // form1_root.panel_market_delta.Height / 2 - form1_root.market_delta_vol - на старте тут будет 0 ибо на старте переменная market_delta_vol = половине выстоы панели. это значение присваивается при старте формы и нужно для того, что бы график рисовался по центру панели

            // подтягивающейся квадратик
            g.FillRectangle(new SolidBrush(Color.Blue), new Rectangle(j, (int)form1_root.market_delta_stop, 50, 6));

            // столбик
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, 0, 255, 0)), new Rectangle(j, (int)form1_root.market_delta_vol + 20, 50, (int)form1_root.market_delta_stop - (int)form1_root.market_delta_vol - 20)); // вверх. 20 и 6 это высота квадратика маркет дельты и стоп лосса маркет дельты
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 0, 0)), new Rectangle(j, (int)form1_root.market_delta_stop + 6, 50, (int)form1_root.market_delta_vol - (int)form1_root.market_delta_stop - 6)); // столбик вниз


            // рисуем мульти линию графика маркет дельты
            Point[] market_delta_array = market_delta_mass.ToArray();
            if (market_delta_mass.Count > 1) // рисуем линю только при наличии 2ух или боле точек. иначе вылезает ошибка
                //g.DrawLines(new Pen(Color.FromArgb(100, 255, 0, 0), 2), points_arry_ask);
                g.DrawLines(new Pen(Color.Red, 1), market_delta_array);


            // рисуем мульти линию графика стоп лосса
            Point[] market_delta_stop_loss_array = market_delta_stop_loss.ToArray();
            if (market_delta_stop_loss.Count > 1) // рисуем линю только при наличии 2ух или боле точек. иначе вылезает ошибка
                //g.DrawLines(new Pen(Color.FromArgb(100, 255, 0, 0), 2), points_arry_ask);
                g.DrawLines(new Pen(Color.Blue, 2), market_delta_stop_loss_array);

        
        } // panel_market_delta_paint
        */




		void ThreadToDo() // поток
		{
			razmetaka_iz_potoka(); // метода добавления контролов (окошек, панелей и др.) из потока

			Random rand = new Random(); // случайное число. использовалось для вставки в строчки стакана для тестирования офф-лайн

			#region первый вариант когда

			while (running)
			{
				//lock (objLocker)
				//{

				//if (!form1_root.smartcom_connect_potok.quote_received) 
				if (true) // писать в массив тольоко после того, как пришла котировка
				{
					//logging.log_add(form1_root, "tape", "strArray.Length ff", "" + strArray.Length, 1);

					for (int j = price_end; j >= price_start; j -= price_step) // заполняем массив элементами
					{
						// тут тоже вылезает ошибка, на клиринеге - индекс за пределами диапазона
						try
						{
							strArray[i] = j.ToString(); // добавляем элементы в массив
						}
						catch (Exception err)
						{
							//mail.send("post@websms.ru", "user=nasled pass=658200 fromPhone=robot-hobot tels=+79108925002 mess=j.ToString(); - индекс за пределами диапазона. время: " + DateTime.Now.ToLongTimeString(), "", false, "C:\\1.txt");
							// на старте бывает начинает писать в окно до его создания
							if (form1_root.stakan.list_view_system != null)
								//form1_root.logging.log_add(form1_root, "system", "class_stacan.cs ThreadToDo()", "strArray[i] = j.ToString(); - индекс за пределами диапазона" + err, 1);
								MessageBox.Show("strArray[i] = j.ToString(); - индекс за пределами диапазона" + err);
							else MessageBox.Show("пишет в окно system, но оно еще не создалось.");
						}
						//strArray[i] = rand.Next(100, 1000).ToString();
						i++;
					}
					i = 0;



				}// if

				//} // lock

				panel_stakan.Invalidate(); // перерисовка стакан панели. после вызова данного метода будут возбуждены соответствующие события Paint


				form1_root.BeginInvoke(new Action(delegate ()
				{
					panel_2.Update(); // апдейт панели под логи
					panel_pod_dlt.Update(); // апдейт панели для маркет дельты

				})); // закрывающая для begin invoke

				que_tick_read(); // читаем очередь тиков
				que_bidask_read(); // читаем очередь линий бид аска

				Thread.Sleep(10); // влияет на загрузку камня. частота обновления стакана она же и частота чтения очередей

			}// while

			#endregion

		} // ThreadToDo()




		void razmetaka_iz_potoka() // метод, который создает контролы исключительно из потока class_stakan
		{
			#region// положение формы и размер + имя окна + событие закрытия формы
			// отображать форму по центру экрана
			form1_root.StartPosition = FormStartPosition.CenterScreen;

			form1_root.BeginInvoke(new Action(delegate ()
			{
				form1_root.Text = "robot_hobot_3"; // имя окна
				form1_root.Location = new Point(0, 0); // положение формы у левого верхнего угла экрана
				form1_root.Height = Screen.GetWorkingArea(form1_root).Height; // высота экрана
				form1_root.Width = Screen.GetWorkingArea(form1_root).Width;

			})); // закрывающая для begin invoke

			// подцепим событие закрытия формы
			form1_root.FormClosed += new FormClosedEventHandler(form1_root_FormClosed);

			#endregion


			#region// метки статус соединения
			label_name = new Label();
			//label_name = "status:";
			Label label_connect = new Label();
			label_connect.Font = new Font("SansSerif", 8);
			label_connect.Text = "status:";
			label_connect.Width = 35;
			label_connect.Height = 12;
			label_connect.Location = new Point(260, 3);
			form1_root.BeginInvoke(new Action(delegate ()
			{
				form1_root.Controls.Add(label_connect);


			})); // закрывающая для begin invoke
			label_connect.BringToFront();



			label_connect_status = new Label();
			label_connect_status.Font = new Font("SansSerif", 8);
			label_connect_status.Text = "0";
			label_connect_status.Height = 12;
			label_connect_status.Location = new Point(295, 3);

			form1_root.BeginInvoke(new Action(delegate ()
			{
				form1_root.Controls.Add(label_connect_status);

			})); // закрывающая для begin invoke

			label_connect_status.BringToFront();
			#endregion


			#region// создадим таб контрол. вкладки в левом верхнем углу, вкладка которого занимает весь экран
			TabControl tab_control_1 = new TabControl();
			tab_control_1.TabPages.Add("main");
			tab_control_1.TabPages.Add("stat");
			tab_control_1.Location = new Point(0, 0);

			form1_root.BeginInvoke(new Action(delegate ()
			{
				typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty).SetValue(tab_control_1, true, null); // зададим свойства что бы не моргало для панели
				tab_control_1.Size = new Size(Screen.GetWorkingArea(form1_root).Width - 5, Screen.GetWorkingArea(form1_root).Height - 35); // 35 - примерно высота таск бара в виндовс хп
				form1_root.Controls.Add(tab_control_1);

			})); // закрывающая для begin invoke
			#endregion




			#region // панель под стакан. внутри этой панели будет еще одна, а на ней - уже стакан
			panel_pod_stakan = new Panel();
			typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty).SetValue(panel_pod_stakan, true, null); // зададим свойства что бы не моргало для панели

			form1_root.BeginInvoke(new Action(delegate () // без инвоука не выходит обратится к свойствам экрана. вылезает ошибка cross-thread
			{
				panel_pod_stakan.Width = Screen.GetWorkingArea(form1_root).Width / 4 + 200;
				panel_pod_stakan.Height = Screen.GetWorkingArea(form1_root).Height - 63;
				panel_pod_stakan.Location = new Point((Screen.GetWorkingArea(form1_root).Width / 4), 0);


				panel_pod_stakan.BackColor = Color.DeepSkyBlue;
				panel_pod_stakan.AutoScroll = true;
				tab_control_1.TabPages[0].Controls.Add(panel_pod_stakan);

			})); // закрывающая для begin invoke


			

			#endregion


			#region// создадим панель на котрой будет распологаться сам стакан

			panel_stakan = new Panel();
			//panel_stakan.Visible = false;
			typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty).SetValue(panel_stakan, true, null); // зададим свойства что бы не моргало для панели
			panel_stakan.Paint += panel_small_paint; // подцепили событие отрисовки малой панели для стакана.
			panel_stakan.Width = 600;
			panel_stakan.Height = 2000;
			panel_stakan.Location = new Point(20, 0); // зададим положение panel_small идентичное положению большой панели
			panel_stakan.BackColor = SystemColors.InactiveCaptionText;
			//panel_stakan.BackColor = Color.Red;
			panel_pod_stakan.Controls.Add(panel_stakan); // добавили на пенель для стакана - саму панель стакана

			#endregion



			#region // панель 2, которая располагается справа от стакана. занимает половину экрана.
			panel_2 = new Panel();
			typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty).SetValue(panel_2, true, null); // зададим свойства что бы не моргало для панели

			form1_root.BeginInvoke(new Action(delegate ()
			{
				panel_2.Location = new Point(Screen.GetWorkingArea(form1_root).Width / 2, 0);
				panel_2.Size = new Size(Screen.GetWorkingArea(form1_root).Width / 2, Screen.GetWorkingArea(form1_root).Height);
			})); // закрывающая для begin invoke

			panel_2.Size = new Size(500, 500);
			panel_2.BackColor = Color.DarkCyan;
			panel_2.AutoScroll = true;
			panel_2.Paint += new PaintEventHandler(panel_2_Paint);
			panel_2.BringToFront();
			tab_control_1.TabPages[0].Controls.Add(panel_2);
			#endregion

			#region// listbox для Tape + подпись
			// попробую добавить лист бокс вместо лист вью. работает
			list_box_tape = new ListBox();

			typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty).SetValue(list_box_tape, true, null); // зададим свойства что бы не моргало для панели

			list_box_tape.DrawItem += new DrawItemEventHandler(list_box_tape_DrawItem);
			list_box_tape.DrawMode = DrawMode.OwnerDrawVariable;


			form1_root.BeginInvoke(new Action(delegate () // без инвоука не выходит обратится к свойствам экрана. вылезает ошибка cross-thread
			{
				list_box_tape.Location = new Point(0 + 200, vkladhi_heidht); // высота от верхней границы tab_control. в этом месте будут располагаться вкладки
				list_box_tape.Width = (Screen.GetWorkingArea(form1_root).Width / 2) / 3 - 200;
				list_box_tape.Height = (Screen.GetWorkingArea(form1_root).Height / 2) - 90;

			})); // закрывающая для begin invoke

			list_box_tape.BringToFront();
			panel_2.Controls.Add(list_box_tape);

			// добавляю подпись
			panel_tape_podpis = new Panel();
			//panel_tape_podpis.Location = new Point(0, 50); // размер и положение панели устанавливается в Paint в соответствии с размером строки названия данной панели
			panel_tape_podpis.BackColor = Color.Chocolate;
			panel_2.Controls.Add(panel_tape_podpis);

			panel_tape_podpis.BringToFront();
			panel_tape_podpis.Paint += new PaintEventHandler(panel_tape_podpis_Paint);
			#endregion




		} // razmetaka_iz_potoka




		void list_box_tape_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e) // событие добавления строчки в лист бокс(окно tape)
		{
			e.DrawBackground();

			Brush myBrush = Brushes.Black;
			Graphics g = e.Graphics;

			string text_box_msg = ((ListBox)sender).Items[e.Index].ToString();
			if (text_box_msg[0] == Convert.ToChar("B")) // в завистимости какая буква в строке, которую выводим в лист бокс - определяем цвет
			{
				g.FillRectangle(new SolidBrush(Color.Chartreuse), e.Bounds); // фон
			}
			else
			{
				g.FillRectangle(new SolidBrush(Color.Red), e.Bounds); // фон
			}

			g.DrawRectangle(new Pen(Color.White), e.Bounds); // рамка

			e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
			e.DrawFocusRectangle();

		}



		public void panel_tape_podpis_Paint(object sender, PaintEventArgs e) // паинт панели tape
		{
			Graphics g = panel_tape_podpis.CreateGraphics(); // создадим графику на панели

			// установим ширину и высоту панели
			panel_tape_podpis.Width = (int)g.MeasureString(tape_podpis_text, podpis_font).Width;
			panel_tape_podpis.Height = podpis_font.Height;
			panel_tape_podpis.Location = new Point(list_box_tape.Width - (int)g.MeasureString(tape_podpis_text, podpis_font).Width + 200, 30);

			g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество
			g.DrawString(tape_podpis_text, podpis_font, Brushes.Black, new Point(0, 0)); // выведем сторку на прямоугольник
		}



		public void panel_filter1_podpis_Paint(object sender, PaintEventArgs e) // паинт панели filter1
		{
			Graphics g = panel_filter1_podpis.CreateGraphics(); // создадим графику на панели

			// установим ширину и высоту панели
			panel_filter1_podpis.Width = (int)g.MeasureString(filter1_podpis_text, podpis_font).Width;
			panel_filter1_podpis.Height = podpis_font.Height;
			panel_filter1_podpis.Location = new Point(list_view_filter1.Location.X + list_view_filter1.Width - (int)g.MeasureString(filter1_podpis_text, podpis_font).Width, 30);

			g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество
			g.DrawString(filter1_podpis_text, podpis_font, Brushes.Black, new Point(0, 0)); // выведем сторку на прямоугольник
		}



		public void panel_filter2_podpis_Paint(object sender, PaintEventArgs e) // паинт панели filter1
		{
			Graphics g = panel_filter2_podpis.CreateGraphics(); // создадим графику на панели

			// установим ширину и высоту панели
			panel_filter2_podpis.Width = (int)g.MeasureString(filter2_podpis_text, podpis_font).Width;
			panel_filter2_podpis.Height = podpis_font.Height;
			panel_filter2_podpis.Location = new Point(list_view_filter2.Location.X + list_view_filter2.Width - (int)g.MeasureString(filter2_podpis_text, podpis_font).Width, 30);

			g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество
			g.DrawString(filter2_podpis_text, podpis_font, Brushes.Black, new Point(0, 0)); // выведем сторку на прямоугольник
		}



		public void panel_system_podpis_Paint(object sender, PaintEventArgs e) // паинт панели system
		{
			Graphics g = panel_system_podpis.CreateGraphics(); // создадим графику на панели

			// установим ширину и высоту панели
			panel_system_podpis.Width = (int)g.MeasureString(system_podpis_text, podpis_font).Width;
			panel_system_podpis.Height = podpis_font.Height;
			panel_system_podpis.Location = new Point(list_view_system.Location.X + list_view_system.Width - (int)g.MeasureString(system_podpis_text, podpis_font).Width, list_box_tape.Height + vkladhi_heidht);



			g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество
			g.DrawString(system_podpis_text, podpis_font, Brushes.Black, new Point(0, 0)); // выведем сторку на прямоугольник
		}



		public void panel_updateorder_podpis_Paint(object sender, PaintEventArgs e) // паинт панели updateorder
		{
			Graphics g = panel_updateorder_podpis.CreateGraphics(); // создадим графику на панели

			// установим ширину и высоту панели
			panel_updateorder_podpis.Width = (int)g.MeasureString(updateorder_podpis_text, podpis_font).Width;
			panel_updateorder_podpis.Height = podpis_font.Height;
			panel_updateorder_podpis.Location = new Point(list_view_updateorder.Location.X + list_view_updateorder.Width - (int)g.MeasureString(updateorder_podpis_text, podpis_font).Width, list_box_tape.Height + vkladhi_heidht);

			g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество
			g.DrawString(updateorder_podpis_text, podpis_font, Brushes.Black, new Point(0, 0)); // выведем сторку на прямоугольник
		}



		public void panel_addtrade_podpis_Paint(object sender, PaintEventArgs e) // паинт панели add_trade
		{
			Graphics g = panel_addtrade_podpis.CreateGraphics(); // создадим графику на панели

			// установим ширину и высоту панели
			panel_addtrade_podpis.Width = (int)g.MeasureString(addtrade_podpis_text, podpis_font).Width;
			panel_addtrade_podpis.Height = podpis_font.Height;
			panel_addtrade_podpis.Location = new Point(list_view_addtrade.Location.X + list_view_addtrade.Width - (int)g.MeasureString(addtrade_podpis_text, podpis_font).Width, list_box_tape.Height + vkladhi_heidht + list_view_system.Height);

			g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество
			g.DrawString(addtrade_podpis_text, podpis_font, Brushes.Black, new Point(0, 0)); // выведем сторку на прямоугольник
		}



		public void panel_trade_podpis_Paint(object sender, PaintEventArgs e) // паинт панели trade
		{
			Graphics g = panel_trade_podpis.CreateGraphics(); // создадим графику на панели

			// установим ширину и высоту панели
			panel_trade_podpis.Width = (int)g.MeasureString(trade_podpis_text, podpis_font).Width;
			panel_trade_podpis.Height = podpis_font.Height;
			panel_trade_podpis.Location = new Point(list_view_trade.Location.X + list_view_trade.Width - (int)g.MeasureString(trade_podpis_text, podpis_font).Width - 12, list_box_tape.Height + vkladhi_heidht + list_view_system.Height); // - 12 добавил потому что подпись уезжала за край эрана. почем то не правильно измеряется длина строки.

			g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество
			g.DrawString(trade_podpis_text, podpis_font, Brushes.Black, new Point(0, 0)); // выведем сторку на прямоугольник
		}



		public void panel_vkladka_1_Paint(object sender, PaintEventArgs e) // паинт панели-эмитатора вкладки №1
		{
			Graphics g = panel_vkladka_1.CreateGraphics(); // создадим графику на панели

			// установим ширину и высоту панели
			panel_vkladka_1.Width = (int)g.MeasureString(panel_vkladka_1_text, podpis_font).Width;
			panel_vkladka_1.Height = podpis_font.Height;
			panel_vkladka_1.Location = new Point(panel_pod_stakan.Location.X + panel_pod_stakan.Width, 0);


			g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество
			g.DrawString(panel_vkladka_1_text, podpis_font, Brushes.Black, new Point(0, 0)); // выведем сторку на прямоугольник

		}



		public void panel_vkladka_2_Paint(object sender, PaintEventArgs e) // паинт панели-эмитатора вкладки №2
		{
			Graphics g = panel_vkladka_2.CreateGraphics(); // создадим графику на панели

			// установим ширину и высоту панели
			panel_vkladka_2.Width = (int)g.MeasureString(panel_vkladka_2_text, podpis_font).Width;
			panel_vkladka_2.Height = podpis_font.Height;
			//panel_vkladka_2.Location = new Point(panel_pod_stakan.Location.X + panel_pod_stakan.Width + (int)g.MeasureString(panel_vkladka_1_text, podpis_font).Width + 5, 0);


			g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество
			g.DrawString(panel_vkladka_2_text, podpis_font, Brushes.Black, new Point(0, 0)); // выведем сторку на прямоугольник

		}



		public void panel_vkladka_3_Paint(object sender, PaintEventArgs e) // паинт панели-эмитатора вкладки №3
		{

			Graphics g = panel_vkladka_3.CreateGraphics(); // создадим графику на панели

			// установим ширину и высоту панели
			panel_vkladka_3.Width = (int)g.MeasureString(panel_vkladka_3_text, podpis_font).Width;
			panel_vkladka_3.Height = podpis_font.Height;
			//panel_vkladka_3.Location = new Point(panel_pod_stakan.Location.X + panel_pod_stakan.Width + (int)g.MeasureString(panel_vkladka_1_text, podpis_font).Width + 5 + (int)g.MeasureString(panel_vkladka_2_text, podpis_font).Width + 5, 0);


			g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество
			g.DrawString(panel_vkladka_3_text, podpis_font, Brushes.Black, new Point(0, 0)); // выведем сторку на прямоугольник

		}



		public void panel_tape_Paint(object sender, PaintEventArgs e)  // паинт панели для вывода всех сделок
		{
			/*
            Graphics g2 = panel_tape.CreateGraphics();
            g2.InterpolationMode = InterpolationMode.HighQualityBilinear; g2.PixelOffsetMode = PixelOffsetMode.HighQuality; g2.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество

            //lock (objLocker)
            //{

                var pt = new Point(0, 0); // отступы от верхнего левого края. x,y x - по горизонтали, y - по вертикали
                for (int i = 0; i < 300; ++i) // проходим по всему массиву. массив на этом моменте уже заполнен
                {
                    g2.DrawString("ffffggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggf", stakan_font, Brushes.Black, pt); // отрисовываем строку. 10ый шрифт (высота 15 пикс), проставляется в мастере
                    pt.Y += 20 + interval_mezh_strok; // смещение по координам вертикально с шагом 3. 3 это расстояние между строчками, от края, в не от середины
                }
            //}
             */





		}















		// события кликов мышкой по панелям эмитатором вкладок

		public void panel_vkladka_1_Click(Object sender, EventArgs e)
		{
			list_box_tape.Visible = true;
			panel_tape_podpis.Visible = true;
			list_view_filter1.Visible = true;
			panel_filter1_podpis.Visible = true;
			list_view_filter2.Visible = true;
			panel_filter2_podpis.Visible = true;
			list_view_system.Visible = true;
			panel_system_podpis.Visible = true;
			list_view_updateorder.Visible = true;
			panel_updateorder_podpis.Visible = true;
			list_view_addtrade.Visible = true;
			panel_addtrade_podpis.Visible = true;
			list_view_trade.Visible = true;
			panel_trade_podpis.Visible = true;
		}

		public void panel_vkladka_2_Click(Object sender, EventArgs e)
		{
			list_box_tape.Visible = false;
			panel_tape_podpis.Visible = false;
			list_view_filter1.Visible = false;
			panel_filter1_podpis.Visible = false;
			list_view_filter2.Visible = false;
			panel_filter2_podpis.Visible = false;
			list_view_system.Visible = false;
			panel_system_podpis.Visible = false;
			list_view_updateorder.Visible = false;
			panel_updateorder_podpis.Visible = false;
			list_view_addtrade.Visible = false;
			panel_addtrade_podpis.Visible = false;
			list_view_trade.Visible = false;
			panel_trade_podpis.Visible = false;



		}

		public void panel_vkladka_3_Click(Object sender, EventArgs e)
		{
			MessageBox.Show("клик по вкладке 3");
		}







		public void panel_2_Paint(object sender, PaintEventArgs e) // паинт теcовой панелия для окон логов
		{

		}


		public void panel_dlt_paint(object sender, PaintEventArgs e) // паинт маркет дельты
		{
			/*
            Graphics g = e.Graphics; // создали графику
            g.InterpolationMode = InterpolationMode.HighQualityBilinear; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.SmoothingMode = SmoothingMode.HighQuality; // что бы было хорошее качество

            g.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 0, 0)), new Rectangle(0, 0, 200, 800)); // столбик вниз


            // работает блять! и не моргает, но тормозит по страшному
            var pt = new Point(0, 0); // отступы от верхнего левого края. x,y x - по горизонтали, y - по вертикали
            for (int i = 0; i < 300; ++i) // проходим по всему массиву. массив на этом моменте уже заполнен
            {
                g.DrawString("ffffggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggf", stakan_font, Brushes.Black, pt); // отрисовываем строку. 10ый шрифт (высота 15 пикс), проставляется в мастере
                pt.Y += 20 + interval_mezh_strok; // смещение по координам вертикально с шагом 3. 3 это расстояние между строчками, от края, в не от середины
            }
            
            */


		}


		public void bid_ask_line(int row, double bid, double bidsize, double ask, double asksize) // заполнение массива для линии бид и аска. вызывается из чтения очереди que_bidask_read(). тоесть que_bidask_read() - читает очеред, данный метод заполняет, а выводится на экран через чтение в цикле в собтии Paint
		{

			// строим массив для линий бида и аска
			if (row == 0) // возьмем первую строчку стакана
			{
				// заполняем массив бид асков
				if (line_points_ask.Count < points_graph_count) // количество точек по которым строим график
				{
					// ask
					line_points_ask.Add(new Point(j, ((int)stakan_font.GetHeight() / 2 + (((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - (int)ask) / price_step)))); // добавляем точки в массив линии ask
																																															// bid
					line_points_bid.Add(new Point(j, ((int)stakan_font.GetHeight() / 2 + (((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - (int)bid) / price_step))));
				}
				else
				{
					// ask
					line_points_ask.RemoveAt(0); // удаляем первый элемент в массиве точек ask
					line_points_ask.Add(new Point(j, ((int)stakan_font.GetHeight() / 2 + (((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - (int)ask) / price_step))));

					//bid
					line_points_bid.RemoveAt(0); // удаляем первый элемент в массиве точек bid
					line_points_bid.Add(new Point(j, ((int)stakan_font.GetHeight() / 2 + (((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - (int)bid) / price_step))));

				} // if


				// смещаем оба массива бида и аска, создаем движение. проходим по всему массиву точек, начиная с 1ой (нулевой) и уменьшаем горизонтальную координату Х - на шаг графика.
				for (int z = 0; z < line_points_ask.Count - 1; z++)
				{
					line_points_ask[z] = new Point(line_points_ask[z].X - points_graph_step, line_points_ask[z].Y); // ask. и сдвигаем горизотальную координату всего массива на определенный шаг. это создает движение. чем больше шаг тем быстрее двигается
					line_points_bid[z] = new Point(line_points_bid[z].X - points_graph_step, line_points_bid[z].Y); // bid
				}

			} //if

		} // bid_ask_line





		public void update_stakan(int row, double bid, double bidsize, double ask, double asksize) // заполнение матрицы стакана
		{

			switch (row) // row
			{
				#region заполнение матрицы бидов и асков. складываем все строки стакана в двумерный массив

				case 0: stakan_mass[0, 0] = (int)asksize; stakan_mass[0, 1] = (int)ask; stakan_mass[0, 2] = (int)bidsize; stakan_mass[0, 3] = (int)bid; break;
				case 1: stakan_mass[1, 0] = (int)asksize; stakan_mass[1, 1] = (int)ask; stakan_mass[1, 2] = (int)bidsize; stakan_mass[1, 3] = (int)bid; break;
				case 2: stakan_mass[2, 0] = (int)asksize; stakan_mass[2, 1] = (int)ask; stakan_mass[2, 2] = (int)bidsize; stakan_mass[2, 3] = (int)bid; break;
				case 3: stakan_mass[3, 0] = (int)asksize; stakan_mass[3, 1] = (int)ask; stakan_mass[3, 2] = (int)bidsize; stakan_mass[3, 3] = (int)bid; break;
				case 4: stakan_mass[4, 0] = (int)asksize; stakan_mass[4, 1] = (int)ask; stakan_mass[4, 2] = (int)bidsize; stakan_mass[4, 3] = (int)bid; break;
				case 5: stakan_mass[5, 0] = (int)asksize; stakan_mass[5, 1] = (int)ask; stakan_mass[5, 2] = (int)bidsize; stakan_mass[5, 3] = (int)bid; break;
				case 6: stakan_mass[6, 0] = (int)asksize; stakan_mass[6, 1] = (int)ask; stakan_mass[6, 2] = (int)bidsize; stakan_mass[6, 3] = (int)bid; break;
				case 7: stakan_mass[7, 0] = (int)asksize; stakan_mass[7, 1] = (int)ask; stakan_mass[7, 2] = (int)bidsize; stakan_mass[7, 3] = (int)bid; break;
				case 8: stakan_mass[8, 0] = (int)asksize; stakan_mass[8, 1] = (int)ask; stakan_mass[8, 2] = (int)bidsize; stakan_mass[8, 3] = (int)bid; break;
				case 9: stakan_mass[9, 0] = (int)asksize; stakan_mass[9, 1] = (int)ask; stakan_mass[9, 2] = (int)bidsize; stakan_mass[9, 3] = (int)bid; break;
				case 10: stakan_mass[10, 0] = (int)asksize; stakan_mass[10, 1] = (int)ask; stakan_mass[10, 2] = (int)bidsize; stakan_mass[10, 3] = (int)bid; break;
				case 11: stakan_mass[11, 0] = (int)asksize; stakan_mass[11, 1] = (int)ask; stakan_mass[11, 2] = (int)bidsize; stakan_mass[11, 3] = (int)bid; break;
				case 12: stakan_mass[12, 0] = (int)asksize; stakan_mass[12, 1] = (int)ask; stakan_mass[12, 2] = (int)bidsize; stakan_mass[12, 3] = (int)bid; break;
				case 13: stakan_mass[13, 0] = (int)asksize; stakan_mass[13, 1] = (int)ask; stakan_mass[13, 2] = (int)bidsize; stakan_mass[13, 3] = (int)bid; break;
				case 14: stakan_mass[14, 0] = (int)asksize; stakan_mass[14, 1] = (int)ask; stakan_mass[14, 2] = (int)bidsize; stakan_mass[14, 3] = (int)bid; break;
				case 15: stakan_mass[15, 0] = (int)asksize; stakan_mass[15, 1] = (int)ask; stakan_mass[15, 2] = (int)bidsize; stakan_mass[15, 3] = (int)bid; break;
				case 16: stakan_mass[16, 0] = (int)asksize; stakan_mass[16, 1] = (int)ask; stakan_mass[16, 2] = (int)bidsize; stakan_mass[16, 3] = (int)bid; break;
				case 17: stakan_mass[17, 0] = (int)asksize; stakan_mass[17, 1] = (int)ask; stakan_mass[17, 2] = (int)bidsize; stakan_mass[17, 3] = (int)bid; break;
				case 18: stakan_mass[18, 0] = (int)asksize; stakan_mass[18, 1] = (int)ask; stakan_mass[18, 2] = (int)bidsize; stakan_mass[18, 3] = (int)bid; break;
				case 19: stakan_mass[19, 0] = (int)asksize; stakan_mass[19, 1] = (int)ask; stakan_mass[19, 2] = (int)bidsize; stakan_mass[19, 3] = (int)bid; break;
				case 20: stakan_mass[20, 0] = (int)asksize; stakan_mass[20, 1] = (int)ask; stakan_mass[20, 2] = (int)bidsize; stakan_mass[20, 3] = (int)bid; break;
				case 21: stakan_mass[21, 0] = (int)asksize; stakan_mass[21, 1] = (int)ask; stakan_mass[21, 2] = (int)bidsize; stakan_mass[21, 3] = (int)bid; break;
				case 22: stakan_mass[22, 0] = (int)asksize; stakan_mass[22, 1] = (int)ask; stakan_mass[22, 2] = (int)bidsize; stakan_mass[22, 3] = (int)bid; break;
				case 23: stakan_mass[23, 0] = (int)asksize; stakan_mass[23, 1] = (int)ask; stakan_mass[23, 2] = (int)bidsize; stakan_mass[23, 3] = (int)bid; break;
				case 24: stakan_mass[24, 0] = (int)asksize; stakan_mass[24, 1] = (int)ask; stakan_mass[24, 2] = (int)bidsize; stakan_mass[24, 3] = (int)bid; break;
				case 25: stakan_mass[25, 0] = (int)asksize; stakan_mass[25, 1] = (int)ask; stakan_mass[25, 2] = (int)bidsize; stakan_mass[25, 3] = (int)bid; break;
				case 26: stakan_mass[26, 0] = (int)asksize; stakan_mass[26, 1] = (int)ask; stakan_mass[26, 2] = (int)bidsize; stakan_mass[26, 3] = (int)bid; break;
				case 27: stakan_mass[27, 0] = (int)asksize; stakan_mass[27, 1] = (int)ask; stakan_mass[27, 2] = (int)bidsize; stakan_mass[27, 3] = (int)bid; break;
				case 28: stakan_mass[28, 0] = (int)asksize; stakan_mass[28, 1] = (int)ask; stakan_mass[28, 2] = (int)bidsize; stakan_mass[28, 3] = (int)bid; break;
				case 29: stakan_mass[29, 0] = (int)asksize; stakan_mass[29, 1] = (int)ask; stakan_mass[29, 2] = (int)bidsize; stakan_mass[29, 3] = (int)bid; break;
				case 30: stakan_mass[30, 0] = (int)asksize; stakan_mass[30, 1] = (int)ask; stakan_mass[30, 2] = (int)bidsize; stakan_mass[30, 3] = (int)bid; break;
				case 31: stakan_mass[31, 0] = (int)asksize; stakan_mass[31, 1] = (int)ask; stakan_mass[31, 2] = (int)bidsize; stakan_mass[31, 3] = (int)bid; break;
				case 32: stakan_mass[32, 0] = (int)asksize; stakan_mass[32, 1] = (int)ask; stakan_mass[32, 2] = (int)bidsize; stakan_mass[32, 3] = (int)bid; break;
				case 33: stakan_mass[33, 0] = (int)asksize; stakan_mass[33, 1] = (int)ask; stakan_mass[33, 2] = (int)bidsize; stakan_mass[33, 3] = (int)bid; break;
				case 34: stakan_mass[34, 0] = (int)asksize; stakan_mass[34, 1] = (int)ask; stakan_mass[34, 2] = (int)bidsize; stakan_mass[34, 3] = (int)bid; break;
				case 35: stakan_mass[35, 0] = (int)asksize; stakan_mass[35, 1] = (int)ask; stakan_mass[35, 2] = (int)bidsize; stakan_mass[35, 3] = (int)bid; break;
				case 36: stakan_mass[36, 0] = (int)asksize; stakan_mass[36, 1] = (int)ask; stakan_mass[36, 2] = (int)bidsize; stakan_mass[36, 3] = (int)bid; break;
				case 37: stakan_mass[37, 0] = (int)asksize; stakan_mass[37, 1] = (int)ask; stakan_mass[37, 2] = (int)bidsize; stakan_mass[37, 3] = (int)bid; break;
				case 38: stakan_mass[38, 0] = (int)asksize; stakan_mass[38, 1] = (int)ask; stakan_mass[38, 2] = (int)bidsize; stakan_mass[38, 3] = (int)bid; break;
				case 39: stakan_mass[39, 0] = (int)asksize; stakan_mass[39, 1] = (int)ask; stakan_mass[39, 2] = (int)bidsize; stakan_mass[39, 3] = (int)bid; break;
				case 40: stakan_mass[40, 0] = (int)asksize; stakan_mass[40, 1] = (int)ask; stakan_mass[40, 2] = (int)bidsize; stakan_mass[40, 3] = (int)bid; break;
				case 41: stakan_mass[41, 0] = (int)asksize; stakan_mass[41, 1] = (int)ask; stakan_mass[41, 2] = (int)bidsize; stakan_mass[41, 3] = (int)bid; break;
				case 42: stakan_mass[42, 0] = (int)asksize; stakan_mass[42, 1] = (int)ask; stakan_mass[42, 2] = (int)bidsize; stakan_mass[42, 3] = (int)bid; break;
				case 43: stakan_mass[43, 0] = (int)asksize; stakan_mass[43, 1] = (int)ask; stakan_mass[43, 2] = (int)bidsize; stakan_mass[43, 3] = (int)bid; break;
				case 44: stakan_mass[44, 0] = (int)asksize; stakan_mass[44, 1] = (int)ask; stakan_mass[44, 2] = (int)bidsize; stakan_mass[44, 3] = (int)bid; break;
				case 45: stakan_mass[45, 0] = (int)asksize; stakan_mass[45, 1] = (int)ask; stakan_mass[45, 2] = (int)bidsize; stakan_mass[45, 3] = (int)bid; break;
				case 46: stakan_mass[46, 0] = (int)asksize; stakan_mass[46, 1] = (int)ask; stakan_mass[46, 2] = (int)bidsize; stakan_mass[46, 3] = (int)bid; break;
				case 47: stakan_mass[47, 0] = (int)asksize; stakan_mass[47, 1] = (int)ask; stakan_mass[47, 2] = (int)bidsize; stakan_mass[47, 3] = (int)bid; break;
				case 48: stakan_mass[48, 0] = (int)asksize; stakan_mass[48, 1] = (int)ask; stakan_mass[48, 2] = (int)bidsize; stakan_mass[48, 3] = (int)bid; break;
				case 49: stakan_mass[49, 0] = (int)asksize; stakan_mass[49, 1] = (int)ask; stakan_mass[49, 2] = (int)bidsize; stakan_mass[49, 3] = (int)bid; break;

					#endregion

			}//switch

			//if (row == 0) logging.log_add(form1_root, "filter1", "update_stakan", "обход стакана", 1);

		} // update_bid_ask


		public void add_tick(double price, double volume, string action) // добавление элементов в массив, по которому раз в 1/10 секунды проходит Paint и выводит соответствующие линии
		{
			//add_tick_market_delta(); // прощет маркет дельты

			// заполняем массив тиков
			if (tick_points.Count < points_graph_count)
			{
				// добавляем точку а массив мультилинии
				tick_points.Add(new Point(j, ((int)stakan_font.GetHeight() / 2 + (((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - (int)price) / price_step)))); // 13 это насколько цена будет посередине строчки

				if (action == "BUY")
				{
					direction_mass.Add("buy");
				}
				else
				{
					direction_mass.Add("sell"); // в зависимости от направления добавим соответствующий элемент в массив
				}

				volume_mass.Add(volume); // добавляем объем данной сделки 

			}
			else
			{
				// удаляем первый элемент в массиве точек, направления и объемов
				// добавляем точки. точка мулти линии. направление и объем - в соответствующие массивы
				tick_points.Add(new Point(j, ((int)stakan_font.GetHeight() / 2 + (((int)stakan_font.GetHeight()) + interval_mezh_strok) * ((price_end - (int)price) / price_step))));
				tick_points.RemoveAt(0);



				if (action == "BUY")
				{
					direction_mass.Add("buy");
				}
				else
				{
					direction_mass.Add("sell"); // в зависимости от направления добавим соответствующий элемент в массив
				}
				direction_mass.RemoveAt(0);


				volume_mass.Add(volume);
				volume_mass.RemoveAt(0);

			}

			// for (int x = 0; x < tick_points.Count - 1 ; x++)
			for (int x = 0; x < tick_points.Count - 1; x++) // проходим по всему массиву точек, начиная с 1ой (нулевой). если убрать -1 то точки не будут касаться стакана. между крайней левой точкой и стаканом будет зазор
			{
				tick_points[x] = new Point(tick_points[x].X - points_graph_step, tick_points[x].Y); // и сдвигаем горизотальную координату всего массива на определенный шаг. это создает движение. чем больше шаг тем быстрее двигается график
			}


			/*
            // заполняем массив направлений сделок
            if (action == StClientLib.StOrder_Action.StOrder_Action_Buy) //определение направления сделки для лонгов
            {
                direction_mass.Add("buy");// для лонгов
            }

            if (action == StClientLib.StOrder_Action.StOrder_Action_Sell)
            {
                direction_mass.Add("sell");// для шортов
            }
            */
		} // add_tick


		void add_tick_market_delta() // просчет маркет дельты
		{

		}


		public int que_elements_procceded = 0;// счетчик обработанных элементов в очереди тиков. нужен для того, что бы не обрабатываьб два раза один и тот же элемент в очереди.
		public int que_bidask_elements_procceded = 1; // счетчих обработанных элементов линий бидасков

		DateTime time = System.DateTime.Now; // переменная для времени. нужна для подсчета кол-ва обработанных тиков в секунду
		public int tick_count_second = 1; // для хранения кол-ва обработанных сделок за секунду

		void que_tick_read() // чтение очереди тиков
		{

			if (form1_root.smartcom_connect_potok.tick_que != null) // проверка на то, успел ли создаться массив очереди
			{
				if (que_elements_procceded < form1_root.smartcom_connect_potok.tick_que.Count) // если кол-во обработаннх элементов меньше кол-ва элементов в очереди
				{
					try
					{
						if (form1_root.smartcom_connect_potok.direction_que[que_elements_procceded] == "BUY") // если лонг
						{
							//form1_root.logging.log_add(form1_root, "listbox", "que_read", "BUY: " + form1_root.smartcom_connect_potok.tick_que[que_elements_procceded - 1] + " vol: " + form1_root.smartcom_connect_potok.volume_que[que_elements_procceded], 1);
						}
						else
						{
							//form1_root.logging.log_add(form1_root, "listbox", "que_read", "SELL: " + form1_root.smartcom_connect_potok.tick_que[que_elements_procceded - 1] + " vol: " + form1_root.smartcom_connect_potok.volume_que[que_elements_procceded], 1);
						}


					}
					catch (Exception err)
					{
						//mail.send("post@websms.ru", "user=nasled pass=658200 fromPhone=robot-hobot tels=+79108925002 mess=ошибка в void que_tick_read() отловлена!", "", false, "C:\\1.txt"); // кому, тело письма, заголовок, путь к вложению C:\\bd.rar
						//form1_root.logging.log_add(form1_root, "system", "void que_tick_read()", "чтение очереди тиков. индекс за пределами диапазона " + err, 1);
						MessageBox.Show(" DomSymbol1.cs line 1510. void que_tick_read()", "чтение очереди тиков. индекс за пределами диапазона. " + err);
					}

					//que_elements_procceded++; // увеличим счетчик обработанных элементов на 1



					// вызываем метод добавления тика в массив. раньше он вызывался на прямую из add_tick смарткома. вызываем метода и передаем в него цену тика, объем и направление.
					try
					{
						add_tick(form1_root.smartcom_connect_potok.tick_que[que_elements_procceded], form1_root.smartcom_connect_potok.volume_que[que_elements_procceded], form1_root.smartcom_connect_potok.direction_que[que_elements_procceded]);
					}
					catch (Exception err)
					{
						//mail.send("post@websms.ru", "user=nasled pass=658200 fromPhone=robot-hobot tels=+79108925002 mess=ошибка в void que_tick_read() add_tick отловлена!", "", false, "C:\\1.txt"); // кому, тело письма, заголовок, путь к вложению C:\\bd.rar
						//form1_root.logging.log_add(form1_root, "system", "void que_tick_read()", "чтение очереди тиков, добавление тика add_tick. индекс за пределами диапазона " + err, 1);

						MessageBox.Show("DomSymbol1.cs line 1521. void que_tick_read()", "чтение очереди тиков, добавление тика add_tick. индекс за пределами диапазона " + err);
					}



					que_elements_procceded++; // увеличим счетчик обработанных элементов на 1

					tick_count_second++;

				} //if


				// используем инвоук ибо это другой процесс. а, из него нельзя получить доступ на изменение элементов формы
				form1_root.BeginInvoke(new Action(delegate ()
				{
					//form1_root.label4.Text = form1_root.smartcom_connect_potok.tick_que.Count.ToString(); // кол-во элементов в очереди
					//form1_root.label6.Text = que_elements_procceded.ToString(); // кол-во обработанных элементов
					//form1_root.label8.Text = (form1_root.smartcom_connect_potok.tick_que.Count - que_elements_procceded).ToString(); // разница того и другого

				})); // invoke

			} // if   

			#region подсчет кол-ва обработаннх тиков за секунду

			// считаем кол-во тиков за 1сек
			if (DateTime.Compare(System.DateTime.Now, time.AddSeconds(1)) < 0) // time раньше time + 1 секунда
			{

			}
			else // прошла секунда
			{
				//logging.log_add(form1_root, "system", "que_read", "обработанно: " + tick_count_second + " поступило: " + form1_root.smartcom_connect_potok.tick_count_second, 3);
				tick_count_second = 1;
				time = System.DateTime.Now;
			} // if

			#endregion

		} //que_read()



		void que_bidask_read() // чтение очереди линий бидасков
		{

			if (form1_root.smartcom_connect_potok.ask_line_que != null) // проверка на то, успел ли создаться массив очереди линий
			{
				if (que_bidask_elements_procceded < form1_root.smartcom_connect_potok.ask_line_que.Count) // если кол-во обработаннх элементов меньше кол-ва элементов в очереди. а, если больше - значит всю очередь уже прочитали
				{
					que_bidask_elements_procceded++; // увеличим счетчик обработанных элементов на 1

					if (form1_root.smartcom_connect_potok.bid_line_que.Count != 0) // пробую убрать ошибку. в момент очистки массивов может вылезать ошибка
																				   // тут тоже вылезает ошибка. индекс за предалми диапазона
						bid_ask_line(0, form1_root.smartcom_connect_potok.bid_line_que[que_bidask_elements_procceded - 1], 0, form1_root.smartcom_connect_potok.ask_line_que[que_bidask_elements_procceded - 1], 0);

				} //if


				// используем инвоук ибо это другой процесс. а, из него нельзя получить доступ на изменение элементов формы
				form1_root.BeginInvoke(new Action(delegate ()
				{
					//form1_root.label5.Text = form1_root.smartcom_connect_potok.ask_line_que.Count.ToString(); // кол-во элементов в очереди
					//form1_root.label7.Text = que_bidask_elements_procceded.ToString(); // кол-во обработанных элементов
					//form1_root.label9.Text = (form1_root.smartcom_connect_potok.ask_line_que.Count - que_bidask_elements_procceded).ToString(); // разница того и другого

				})); // invoke

			} // if   


		} // que_bidask_read()










		void connect_button_Click(Object sender, EventArgs e) // обработка события кнопки подключиться
		{
			

		} // disconnect_button_Click

		void disconnect_button_Click(Object sender, EventArgs e) // обработка события кнопки отключиться
		{
			 

		} // connect_button_Click


		void form1_root_FormClosed(object sender, FormClosedEventArgs e) // обработка события закрытия формы
		{
			

		} // orm1_root_FormClosed()




	} // class_stakan
}

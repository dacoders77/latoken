using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using BitMEXAssistant.Properties;

namespace BitMEXAssistant {
    public class OrderBookControl : Control {
        private readonly int _chartWidth = 400; // положение по Х, горизонтальная координата. расстояние от левого края панели до стакана. ширина поля на котором рисуется график
        private readonly int _intervalMezhStrok = 4; // интервал между строк
        private readonly int _dlinaFonaZayav = 54; // длина поля стакана, выделяемое под заявки. данная переменная указывается руками далее вычисляется длина строки (dlina_fona_stroki) ибо неизвестно сколько знаков в цене инструмента, которым торгуем. потом данные переменные складываются и получается ширина стакана.
        private readonly int suzhenie_fona = 0; // насколько будет сужен прямоугольник используемый в качестве фона. отрицательные значение (-5) вызовет расширение фона
        private readonly bool showBorder = false; // отображать рамку
        private readonly int limit_size_1 = 10; // пороги вывода кружочков на сделках (объем). 3 варианта: 1ый < 1ый порог. 2ой: > 1ого < ого. 3ий: > 2ого. когда прошла сделка с объемом < 1ого порога - кружочик выводится в виде точки
        private readonly int limit_size_2 = 100;
        private readonly int _markSize1 = 6; // 3 вида кругов для соответствующих объемом
        private readonly int _markSize2 = 16;
        private readonly int _markSize3 = 24;
        private readonly int _pointsGraphCount = 38; // количество точек по которым строим график
        private readonly int _pointsGraphStep = 30; // шаг точек графика по горизонтали. 

        private readonly Font _markFont = new Font("Calibri", 10, FontStyle.Bold); // определим шрифт для кружочков

        private readonly List<Tick> _ticks = new List<Tick>();
        private readonly List<Point> _askChartPoints = new List<Point>(); // коллекция для хранения точек для построения линии ASK. заполняеится из update_bid_ask
        private readonly List<Point> _bidChartPoints = new List<Point>(); // // коллекция для хранения точек для построения линии BID. заполняеится из update_bid_ask
        private OrderBookDataSet _dataSet;

        private bool _scrollNeeded = true; // флаг для прокрутки стакана к той цене, которая торгуется. выполняется один раз

        private readonly Pen _ticksPen = new Pen(Color.FromArgb(200, 0, 0, 0), 3);
        private decimal _priceStart;
        private decimal _priceEnd;
        private decimal _priceStep = new decimal(0.1);
        private readonly SoundPlayer _tradeSound;

        public OrderBookControl() {
            DoubleBuffered = true;
            Font = new Font("Calibri", 10); // шрифт в строчках стакана
            
            _tradeSound = new SoundPlayer(Resources.tick_2);
        }

        protected override void OnPaint(PaintEventArgs e) {
            if (_priceEnd <= _priceStart || _priceStep <= 0)
                return;

            var g = e.Graphics;

            // что бы было хорошее качество
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (DesignMode)
                return;

            DrawPriceBar(g, DataSet);

            DrawLineChart(g, _askChartPoints, Pens.Red);

            DrawLineChart(g, _bidChartPoints, Pens.Green);

            DrawTicks(g, _ticks);

            ScrollIfNeeded();

            base.OnPaint(e);
        }

        // TODO 
        private void ScrollIfNeeded() { // скролл панелей до видимой зоны
            if (_dataSet == null)
                return;

            // промотаем стакан к торгуемой цене. проматываем только тогда, когда пришли планци цены. 
            // когда появились цены в матрице стакана - значит и планки пришли. проверяем цену 0-вой строчки на неравенство 0
            var sampleVolume = _dataSet.Bid[0].Volume;
            if (_scrollNeeded && sampleVolume != 0) {
                //form1_root.panel_big.AutoScrollPosition = new Point(0, (int) (((int)Font.GetHeight() + _intervalMezhStrok) * ((price_end - sampleValume) / price_step) - 400));
                // обратимся в переменную market_delta_vol, там содержится координата на которой будет рисоваться график. 
                // начальное ее значение = верхняя планка - нижняя / 2. прибавляем к этой переменной еще пол высоты панели, что бы бло по середине. 
                // без этого получается координата верхнего края. стоит знак -. что бы передвинуть график к центру - скрол нужно уменьшать
                _scrollNeeded = false;
            }
        }

        private void DrawTicks(Graphics g, IReadOnlyCollection<Tick> ticks) {
            // рисуем график тиков (сделок)
            var tickPoints = ticks.Select(t => t.Position).ToArray();
            if (tickPoints.Length > 1)
                g.DrawLines(_ticksPen, tickPoints.ToArray());

            // рисуем кружочки
            foreach (var tick in ticks) {
                int markSize; // размер круга. рассчитывается на ходу в зависимости от направления и объема сделки

                // определение размера кружочка в зависимости от объема
                if (tick.Volume <= limit_size_1)
                    markSize = _markSize1;
                else if (tick.Volume < limit_size_2)
                    markSize = _markSize2;
                else
                    markSize = _markSize3;

                var showValue = tick.Volume > limit_size_1;

                g.DrawEllipse(tick.IsBuy ? Pens.Green : Pens.LightCoral, tick.Position.X - markSize / 2, tick.Position.Y - markSize / 2, markSize, markSize);
                g.FillEllipse(tick.IsBuy ? Brushes.LimeGreen : Brushes.LightPink, tick.Position.X - markSize / 2, tick.Position.Y - markSize / 2, markSize, markSize);

                // цифра в кружочке
                if (showValue)
                    // выводим объем текстом. что бы определить куда нужно выводить строку - нужно ее длину поделить пополам и результат вычесть из точки. длина строки зависит от того, каким шрифтом пишем
                    g.DrawString(tick.Volume.ToString(), _markFont, Brushes.Black, tick.Position.X - g.MeasureString(tick.Volume.ToString(), _markFont).Width / 2, tick.Position.Y - _markFont.Height / 2);
            }
        }

        private static void DrawLineChart(Graphics g, IReadOnlyCollection<Point> points, Pen pen) {
            if (points.Count > 1)
                g.DrawLines(pen, points.ToArray());
        }

        private void DrawPriceBar(Graphics g, OrderBookDataSet dataSet) { // рисуем цены инструмента в столбик
            if (dataSet == null)
                return;

            var fontHeight = (int)Font.GetHeight();

            var y = 0;
            for (var p = _priceStart; p < _priceEnd; p += _priceStep) {
                g.DrawString(p.ToString(CultureInfo.CurrentCulture), Font, Brushes.LightGray, _chartWidth, y);
                y += fontHeight + _intervalMezhStrok;
            }

            // подсчитаем длину строки. залезем в первую строчку матрицы бид и аска и возьмем оттуда значение. 
            // смысл этого в том, что мы не знаем скоько знаков в цене инструмента. для это и лезем
            var dlinaFonaStroki = _dlinaFonaZayav + (int)g.MeasureString(dataSet.Ask[0].Price.ToString(), Font).Width;

            foreach (var record in dataSet.Ask)
                DrawPriceBarRow(g, record, dlinaFonaStroki, fontHeight, Brushes.LightPink);
            foreach (var record in dataSet.Bid)
                DrawPriceBarRow(g, record, dlinaFonaStroki, fontHeight, Brushes.LimeGreen);
        }

        private void DrawPriceBarRow(Graphics g, OrderBookRecord record, int dlinaFonaStroki, int fontHeight, Brush baseBackground) {
            // рассчитываем ширину и цвет столбика, который отображает объем заявок в цене
            var askBackgroundWidth = GetPriceBarColumnLength(record.Volume, dlinaFonaStroki);
            var askBackground = GetPriceBarColumnBrush(record.Volume, dlinaFonaStroki);

            var y = (int)((fontHeight + _intervalMezhStrok) * ((_priceEnd - record.Price) / _priceStep));

            // общий фон строки
            g.FillRectangle(baseBackground, _chartWidth, y + suzhenie_fona / 2, dlinaFonaStroki, fontHeight - suzhenie_fona);
            // полосочка под заявки. меняет цвет в зависимости от объема.
            g.FillRectangle(askBackground, _chartWidth, y + suzhenie_fona / 2, askBackgroundWidth, fontHeight - suzhenie_fona);

            // цена ячейки + объем
            g.DrawString(record.Price.ToString(), Font, Brushes.Black, _chartWidth, y);
            g.DrawString(record.Volume.ToString(), Font, Brushes.Black, _chartWidth + _dlinaFonaZayav, y);

            if (showBorder)
                g.DrawRectangle(
                    Pens.Black,
                    _chartWidth,
                    y + suzhenie_fona / 2,
                    (int)g.MeasureString(record.Price + "  " + record.Volume, Font).Width + _intervalMezhStrok, // TODO проверить что не перепутано
                    fontHeight - suzhenie_fona);
        }

        private static Brush GetPriceBarColumnBrush(decimal price, int treshold) {
            var priceSqrt = Math.Sqrt((double)price);
            if (priceSqrt < treshold / 2) // проверяем объем который вылезает за половину рассчитанной ширины стакана
                return Brushes.Khaki;
            if (priceSqrt < treshold)
                return Brushes.Tomato;
            // если объем больше половины ширины
            return Brushes.Red;
        }

        private static int GetPriceBarColumnLength(decimal price, int treshold) {
            var priceSqrt = Math.Sqrt((double)price);
            if (priceSqrt < treshold / 2) // проверяем объем который вылезает за половину рассчитанной ширины стакана
                return (int)priceSqrt;
            // если объем больше половины ширины
            return treshold;
        }

        public void AddTrade(TradeData data) {
            if (_ticks.Count > _pointsGraphCount)
                _ticks.RemoveAt(0);

            var fontHeight = (int)Font.GetHeight();
            _ticks.Add(
                new Tick {
                    Position = new Point(
                        _chartWidth,
                        GetY(data.Price, fontHeight)
                    ),
                    IsBuy = data.Direction == TradeDirection.Buy,
                    Volume = data.Volume
                }
            );

            for (int i = 0; i < _ticks.Count - 1; i++) {
                _ticks[i].Position = new Point(_ticks[i].Position.X - _pointsGraphStep, _ticks[i].Position.Y);
            }

            _tradeSound.Play();
            
            Invalidate();
        }

        private void UpdateChartPoints(OrderBookDataSet dataSet) {
            var fontHeight = (int)Font.GetHeight();

            if (_askChartPoints.Count >= _pointsGraphCount) {
                _askChartPoints.RemoveAt(0); // удаляем первый элемент в массиве точек ask
                _bidChartPoints.RemoveAt(0); // удаляем первый элемент в массиве точек bid
            }

            if (dataSet != null) {
                _askChartPoints.Add(new Point(_chartWidth, GetY(dataSet.Ask[0].Price, fontHeight)));
                _bidChartPoints.Add(new Point(_chartWidth, GetY(dataSet.Bid[0].Price, fontHeight)));
            }

            // смещаем оба массива бида и аска, создаем движение. проходим по всему массиву точек, начиная с 1ой (нулевой) и уменьшаем горизонтальную координату Х - на шаг графика.
            for (int i = 0; i < _askChartPoints.Count - 1; i++) {
                _askChartPoints[i] = new Point(_askChartPoints[i].X - _pointsGraphStep, _askChartPoints[i].Y); // ask. и сдвигаем горизотальную координату всего массива на определенный шаг. это создает движение. чем больше шаг тем быстрее двигается
                _bidChartPoints[i] = new Point(_bidChartPoints[i].X - _pointsGraphStep, _bidChartPoints[i].Y); // bid
            }

            Invalidate();
        }

        private int GetY(decimal price, int fontHeight) {
            return (int) (fontHeight / 2 + (fontHeight + _intervalMezhStrok) * ((_priceEnd - price) / _priceStep));
        }


        public OrderBookDataSet DataSet {
            get { return _dataSet; }
            set {
                _dataSet = value;

                UpdateChartPoints(_dataSet);
            }
        }

        private void UpdateHeight() {
            if (_priceEnd <= _priceStart || _priceStep <= 0)
                return;

            var count = (_priceEnd - _priceStart) / _priceStep;
            if (count <= 1)
                return;

            var height = count * ((int) Font.GetHeight() + _intervalMezhStrok);
            Size = new Size(Size.Width, (int) height);
        }

        protected override void OnFontChanged(EventArgs e) {
            UpdateHeight();
            base.OnFontChanged(e);
        }

        public decimal PriceEnd {
            get { return _priceEnd; }
            set {
                _priceEnd = value;
                UpdateHeight();
            }
        }

        public decimal PriceStart {
            get { return _priceStart; }
            set {
                _priceStart = value; 
                UpdateHeight();
            }
        }

        public decimal PriceStep {
            get { return _priceStep; }
            set {
                _priceStep = value;
                UpdateHeight();
            }
        } // шаг цены инструмента. RTS - 10

        private class Tick {
            public Point Position { get; set; }
            public bool IsBuy { get; set; }
            public double Volume { get; set; }
        }
    }
}
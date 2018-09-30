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
        private readonly int _chartWidth = 400; // Horizontal coordinate. Location on X axis. Width of the field on which the chart is rendered
		private readonly int _verticalPriceBarInterval = 4; // Price bars vertical interval. The length between price bars
        private readonly int _orderBackgroundWidth = 54; // Order background width. We don't know how many digits are in the price of traded symbol. Based on this value the width of the DOM is calculated
		private readonly int _backGroundReduction = 0; // Background size reduction. The value on which the background of the price bar is reduced. Negative value will expand the background
        private readonly int _volumeSizeThreshold1 = 10; // Volume size circles thresholds. There are 3 sizes: 1st < 1st threshold. 2nd: > 1st < 2rd. 3rd: > 2nd
        private readonly int _volumeSizeThreshold2 = 100;

        private readonly int _markSize1 = 6; // 3 sizes of circles for 3 volume groups
        private readonly int _markSize2 = 16;
        private readonly int _markSize3 = 31;

        private readonly int _pointsGraphCount = 38; // The quantity of points used for rendering the chart
        private readonly int _pointsGraphStep = 30; // Points horisontal step 

        private readonly Font _markFont = new Font("Calibri", 10, FontStyle.Bold); // Circle font

        private readonly List<Tick> _ticks = new List<Tick>();
        private readonly List<Point> _askChartPoints = new List<Point>(); // The collection used for ask line chart render
        private readonly List<Point> _bidChartPoints = new List<Point>(); 
        private OrderBookDataSet _dataSet;

        private readonly Pen _ticksPen = new Pen(Color.FromArgb(200, 0, 0, 0), 3);

        private decimal _priceStart; // Visble DOM stack starting price
        private decimal _priceEnd; // Ending price
        private decimal _priceStep; // Price bar step. ETHUSD: 0.1, XBTCUSD: 1


        private readonly SoundPlayer _tradeSound; // The sound played when a trade is being executed
        private List<Order> _activeOrders;

        public OrderBookControl() {
            DoubleBuffered = true;
            Font = new Font("Calibri", 10); // Font for price bars
            
            _tradeSound = new SoundPlayer(Resources.tick_2);
        }

        protected override void OnPaint(PaintEventArgs e) {
            if (_priceEnd <= _priceStart || _priceStep <= 0)
                return;

            var g = e.Graphics;

            // For better quality
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (DesignMode)
                return;

            DrawPriceBar(g, DataSet);
            DrawLineChart(g, _askChartPoints, Pens.Red);
            DrawLineChart(g, _bidChartPoints, Pens.Green);
            DrawTicks(g, _ticks);

            base.OnPaint(e);
        }
		
        private void DrawTicks(Graphics g, IReadOnlyCollection<Tick> ticks) {

            // Draw ticks chart
            var tickPoints = ticks.Select(t => t.Position).ToArray();
            if (tickPoints.Length > 1)
                g.DrawLines(_ticksPen, tickPoints.ToArray());

			// Draw circles
			foreach (var tick in ticks.ToList())
			{
				int markSize; // Circle size is calculated in the run

				// Calculate circle size based on the trade's volume 
				if (tick.Volume <= _volumeSizeThreshold1)
					markSize = _markSize1;
				else if (tick.Volume < _volumeSizeThreshold2)
					markSize = _markSize2;
				else
					markSize = _markSize3;

				var showValue = tick.Volume > _volumeSizeThreshold1;

				g.DrawEllipse(tick.IsBuy ? Pens.Green : Pens.LightCoral, tick.Position.X - markSize / 2, tick.Position.Y - markSize / 2, markSize, markSize);
				g.FillEllipse(tick.IsBuy ? Brushes.LimeGreen : Brushes.LightPink, tick.Position.X - markSize / 2, tick.Position.Y - markSize / 2, markSize, markSize);

				// Digit in a circle
				if (showValue)
					// Output volume as a text. In order to determine where to output a line - it's length should be divided on half and obtained result exclude from the point. The length of the bar is dependent on the font size
					g.DrawString(tick.Volume.ToString(), _markFont, Brushes.Black, tick.Position.X - g.MeasureString(tick.Volume.ToString(), _markFont).Width / 2, tick.Position.Y - _markFont.Height / 2);
			}
		}

		private static void DrawLineChart(Graphics g, IReadOnlyCollection<Point> points, Pen pen) {
            if (points.Count > 1)
                g.DrawLines(pen, points.ToArray());
        }

		// Draw a vertical stack of the prices 
		private void DrawPriceBar(Graphics g, OrderBookDataSet dataSet) { 
            if (dataSet == null)
                return;

            var fontHeight = (int)Font.GetHeight();

            for (var p = _priceStart; p < _priceEnd; p += _priceStep) {
                g.DrawString(p.ToString(CultureInfo.CurrentCulture), Font, Brushes.LightGray, _chartWidth, GetY(p, fontHeight) - fontHeight / 2);
            }

		    var maxVolume = Math.Max(dataSet.Ask.Max(d => d.Volume), dataSet.Bid.Max(d => d.Volume));

            // Price bar background width calculation 
            var priceBarBackGroundWidth = _orderBackgroundWidth + (int)g.MeasureString(maxVolume.ToString(), Font).Width;

		    // Render ask (uppder) part of the DOM
            foreach (var record in dataSet.Ask)
                DrawPriceBarRow(g, record, priceBarBackGroundWidth, fontHeight, Brushes.LightPink, maxVolume);

			// Draw bid (lower) part of the DOM
            foreach (var record in dataSet.Bid)
                DrawPriceBarRow(g, record, priceBarBackGroundWidth, fontHeight, Brushes.LimeGreen, maxVolume);

            if(ActiveOrders != null)
		        foreach (var activeOrder in ActiveOrders) {
		            g.DrawRectangle(Pens.Black, _chartWidth, GetY(activeOrder.Price, fontHeight) - fontHeight / 2, priceBarBackGroundWidth, fontHeight - _backGroundReduction);
		        }
		}

        private void DrawPriceBarRow(Graphics g, OrderBookRecord record, int priceBarBackGroundWidth, int fontHeight, Brush baseBackground, int maxVolume) {
			// Calculate width and color of the small bar, which represents volume in a whole price bar (price row)
            var askBackgroundWidth = GetPriceBarColumnLength(record.Volume, priceBarBackGroundWidth, maxVolume);
            var askBackground = GetPriceBarColumnBrush(record.Volume, maxVolume);

            var y = GetY(record.Price, fontHeight) - fontHeight / 2;

            // Price bar background
            g.FillRectangle(baseBackground, _chartWidth, y + _backGroundReduction / 2, priceBarBackGroundWidth, fontHeight - _backGroundReduction);
           
			// Volume bar in the whole price bar changes its color based in the volume
			// *****************************************************************************
			// RED BACKGROUND ON A BIG VOLUME
            g.FillRectangle(askBackground, _chartWidth, y + _backGroundReduction / 2, askBackgroundWidth, fontHeight - _backGroundReduction);

			// Price bar price + volume
            g.DrawString(record.Price.ToString(), Font, Brushes.Black, _chartWidth, y);
            g.DrawString(record.Volume.ToString(), Font, Brushes.Black, _chartWidth + _orderBackgroundWidth, y);
        }

        private static Brush GetPriceBarColumnBrush(int volume, int maxVolume) {
            //var volumeSqrt = (int) Math.Sqrt(volume);
            if (volume < maxVolume / 2) // Check the volume which is greater than half of the calculated width of the DOM
                return Brushes.Khaki;
            if (volume < maxVolume)
                return Brushes.Tomato;
			// If the size is greater than a half - make it RED!
			return Brushes.Red;
			
        }

        private static int GetPriceBarColumnLength(int volume, int maxWidth, int maxVolume) {
            //var volumeSqrt = (int) Math.Sqrt(volume);
            if (volume >= maxVolume)
                return maxWidth;

            return (int) (volume / (double) maxVolume * maxWidth);
        }

        public void AddTrade(TradeData data) {

			//Console.WriteLine("OrderBookControl.cs line 184: " + data.Direction + " " + data.Price + " " + data.Volume);

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

            if (SoundEnabled)
                _tradeSound.Play(); // Play sound in each tick 
            
            Invalidate();
        }

        private void UpdateChartPoints(OrderBookDataSet dataSet) {
            var fontHeight = (int)Font.GetHeight();

            if (_askChartPoints.Count >= _pointsGraphCount) {
                _askChartPoints.RemoveAt(0); // Remove the first element from ask array
				_bidChartPoints.RemoveAt(0); // From bid
			}

            if (dataSet != null) {
                _askChartPoints.Add(new Point(_chartWidth, GetY(dataSet.Ask[0].Price, fontHeight)));
                _bidChartPoints.Add(new Point(_chartWidth, GetY(dataSet.Bid[0].Price, fontHeight)));
            }

			// Shit both arrays (bid, ask), make a visual move. Run throughout the array (all points) starting from the 1st and decrease X coordinate with the step
            for (int i = 0; i < _askChartPoints.Count - 1; i++) {
                _askChartPoints[i] = new Point(_askChartPoints[i].X - _pointsGraphStep, _askChartPoints[i].Y); // ask. Shift a horisontal coordinate of the whole array with a step. This makes a move. Bigger step - faster move
				_bidChartPoints[i] = new Point(_bidChartPoints[i].X - _pointsGraphStep, _bidChartPoints[i].Y); // bid 
            }

            Invalidate();
        }

        private int GetY(decimal price, int fontHeight) {
            return (int) ((fontHeight + _verticalPriceBarInterval) * ((_priceEnd - price) / _priceStep));
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

            var height = count * ((int) Font.GetHeight() + _verticalPriceBarInterval);
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
        }

        public bool SoundEnabled { get; set; } = true;

        public List<Order> ActiveOrders {
            get { return _activeOrders; }
            set {
                _activeOrders = value; 

                Invalidate();
            }
        }

        private class Tick {
            public Point Position { get; set; }
            public bool IsBuy { get; set; }
            public double Volume { get; set; }
        }
    }
}
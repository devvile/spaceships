using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Security.Cryptography;
using System.ComponentModel;


namespace NinjaTrader.NinjaScript.Strategies
{
    public class Sojuz : Strategy
    {
        #region declarations

        private Indicator _aroon;
        private Indicator _fastHMA;
        private Indicator _slowHMA;
        private Indicator _ha;


        private int _FastHMAPeriod;
        private int _SlowHMAPeriod;


        private double _aroonDown;
        private double _aroonUp;


        private Order _longOneOrder;
        private Order _longTwoOrder;
        private double _longEntryPrice1;
        private double _longTarget1;


        private Order _shortOneOrder;
        private Order _shortTwoOrder;
        private double _shortEntryPrice1;
        private double _shortTarget1;


        private double _longStopLoss2;

        private double _stopLossBaseLong;
        private double _stopLossBaseShort;
        #endregion

        #region My Parameters


        private int _aroonPeriod = 25;

        private bool _useLongs = true;
        private bool _useShorts = true;
        private bool _useAroon = true;
        private bool _useHMA = true;
        private bool _showHA = true;
        private bool _canTrade = true;

        private int _lotSize1 = 1;
        private int _lotSize2 = 1;

        private int _stopLong = 40;
        private int _stopShort = 40;

        private int BarNr = 0;  //sprawdzic <-----
        Random rnd = new Random();

        private string status = "Flat";


        #endregion


        [Display(Name = "Show HA", GroupName = "CONFIG", Order = 0)]
        public bool ShowHA
        {
            get { return _showHA; }
            set { _showHA = value; }
        }

        #region Longs

        [Display(Name = "Use Longs", GroupName = "LONGS", Order = 0)]
        public bool UseLongs
        {
            get { return _useLongs; }
            set { _useLongs = value; }
        }



        [Display(Name = "Stop Loss", GroupName = "LONGS", Order = 0)]
        public int StopLong
        {
            get { return _stopLong; }
            set { _stopLong = value; }
        }


        #endregion

        #region Shorts

        [Display(Name = "Use Shorts", GroupName = "SHORTS", Order = 0)]
        public bool UseShorts
        {
            get { return _useShorts; }
            set { _useShorts = value; }
        }

        [Display(Name = "Stop Loss", GroupName = "SHORTS", Order = 0)]
        public int StopShort
        {
            get { return _stopShort; }
            set { _stopShort = value; }
        }



        #endregion

        #region Position Management

        [Display(Name = "Position Size 1", GroupName = "Position Management", Order = 0)]
        public int LotSize1
        {
            get { return _lotSize1; }
            set { _lotSize1 = value; }
        }

        [Display(Name = "Position Size 2", GroupName = "Position Management", Order = 0)]
        public int LotSize2
        {
            get { return _lotSize2; }
            set { _lotSize2 = value; }
        }




        #endregion

        #region Filters

        [Display(Name = "Aroon Period", GroupName = "Filters", Order = 0)]
        public int AroonPeriod
        {
            get { return _aroonPeriod; }
            set { _aroonPeriod = value; }
        }

        [Display(Name = "Use Aroon", GroupName = "Filters", Order = 0)]
        public bool UseAroon
        {
            get { return _useAroon; }
            set { _useAroon = value; }
        }


        [Display(Name = "Use HMA", GroupName = "Filters", Order = 0)]
        public bool UseHMA
        {
            get { return _useHMA; }
            set { _useHMA = value; }
        }

        [Display(Name = "HMA Fast Period", GroupName = "Filters", Order = 0)]
        public int FastHMAPeriod
        {
            get { return _FastHMAPeriod; }
            set { _FastHMAPeriod = value; }
        }

        [Display(Name = "HMA SLow Period", GroupName = "Filters", Order = 0)]
        public int SlowHMAPeriod
        {
            get { return _SlowHMAPeriod; }
            set { _SlowHMAPeriod = value; }
        }

        #endregion



        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Sojuz";
                Name = "Sojuz";
                Calculate = Calculate.OnBarClose;

            }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;

                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();
                AddIndicators();
                Calculate = Calculate.OnBarClose;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade) return;

            if (Position.MarketPosition == MarketPosition.Flat)
            {
                status = "Flat";

            }

    //        CalculateStop();
            CalculateTradeTime();

            if (Position.MarketPosition == MarketPosition.Long && CrossAbove(_slowHMA, _fastHMA, 1)) 
            {
                ExitLong("Long1");
            }else if(Position.MarketPosition == MarketPosition.Long && CrossAbove(Aroon(AroonPeriod).Down, Aroon(AroonPeriod).Up, 1))
            {
        //        Mark("Short");
                ExitLong("Long2");
                ExitLong("Long1");
            }

            if (Position.MarketPosition == MarketPosition.Short && CrossAbove(_fastHMA, _slowHMA, 1))
            {
                ExitShort("Short1");
            }
            else if (Position.MarketPosition == MarketPosition.Short && CrossAbove(Aroon(AroonPeriod).Up, Aroon(AroonPeriod).Down, 1))
            {
       //         Mark("Long");
                ExitShort("Short2");
                ExitShort("Short1");
            }

            // Entry
     
            if (LongConditions() && UseLongs)
            {
                Mark("Long");
                _longOneOrder = EnterLong(LotSize1, "Long1");
                _longTwoOrder = EnterLong(LotSize2, "Long2");
            }
            if (ShortConditions() && UseShorts)
            {
                Mark("Short");
                _shortOneOrder = EnterShort(LotSize1, "Short1");
                _shortTwoOrder = EnterShort(LotSize2, "Short2");
            }
        }

        private void CalculateStop()
        {
            if(Position.MarketPosition == MarketPosition.Long)
            {
                SetStopLoss(CalculationMode.Ticks, StopLong);
            }else if (Position.MarketPosition == MarketPosition.Short)
            {
                SetStopLoss(CalculationMode.Ticks, StopShort);
            }
        }

        private bool LongConditions()
        {
            return aroonLong() && hmaLong() && _canTrade && noPositions();
        }

        private bool ShortConditions()
        {
            return aroonShort() && hmaShort() && _canTrade && noPositions();
        }

        private void Mark(string positionType)
        {
            int _nr = rnd.Next();
            string rando = Convert.ToString(_nr);
            string name = "tag " + rando;
            string name2 = "tag2 " + rando;

            if (positionType == "Short")
            {
                Draw.ArrowDown(this, name, true, 0, High[0] + 4 * TickSize, Brushes.Red);
            }

            else if (positionType == "Long")
            {
                Draw.ArrowUp(this, name, true, 0, Low[0] - 4 * TickSize, Brushes.Blue);
            }

            else if (positionType == "Strong Short")
            {
                Draw.ArrowDown(this, name, true, 0, High[0] + TickSize, Brushes.Red);
                Draw.Text(this, name2, "SS", 0, High[0] + 12);
            }

            else if (positionType == "Weak Short")
            {
                Draw.ArrowDown(this, name, true, 0, High[0] + TickSize, Brushes.Yellow);
                Draw.Text(this, name2, "WS", 0, High[0] + 12);
            }
            else if (positionType == "Strong Long")
            {
                Draw.ArrowUp(this, name, true, 0, Low[0] - TickSize, Brushes.Blue);
                Draw.Text(this, name2, "SLONG", 0, Low[0] - 12 * TickSize);
            }

            else if (positionType == "Weak Long")
            {
                Draw.ArrowUp(this, name, true, 0, Low[0] - TickSize, Brushes.Yellow);
                Draw.Text(this, name2, "WL", 0, Low[0] - 12 * TickSize);
            }

        }


        private bool aroonLong()
        {
            if(UseAroon)
            {
                return CrossAbove(Aroon(AroonPeriod).Up, Aroon(AroonPeriod).Down, 1);
            }
            else
            {
                return true;
            }

        }
        private bool aroonShort()
        {
            if (UseAroon)
            {
                return CrossAbove(Aroon(AroonPeriod).Down, Aroon(AroonPeriod).Up, 1);
            }
            else
            {
                return true;
            }

        }

        private bool hmaLong()
        {
            return _fastHMA[0]> _slowHMA[0];
        }

        private bool hmaShort()
        {
            return _fastHMA[0] < _slowHMA[0];
        }

        private bool noPositions()
        {
            return Position.MarketPosition == MarketPosition.Flat;
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (OrderFilled(order) && IsLongOrder1(order))
            {
                SetStopLoss("Long1", CalculationMode.Ticks, StopLong, false);

            }
            else if (OrderFilled(order) && IsLongOrder2(order))
            {
                SetStopLoss("Long2", CalculationMode.Ticks, StopLong, false);
            }
            

            else if (OrderFilled(order) && IsShortOrder1(order))
            {
                SetStopLoss("Short1", CalculationMode.Ticks, StopShort, false);
            }
            else if (OrderFilled(order) && IsShortOrder2(order))
            {
                SetStopLoss("Short2", CalculationMode.Ticks, StopShort, false);
            }

        }

        private bool IsLongOrder1(Order order)
        {
            return order == _longOneOrder;
        }

        private bool IsLongOrder2(Order order)
        {
            return order == _longTwoOrder;
        }

        private bool IsShortOrder1(Order order)
        {
            return order == _shortOneOrder;
        }

        private bool IsShortOrder2(Order order)
        {
            return order == _shortTwoOrder;
        }

        private bool OrderFilled(Order order)
        {
            return order.OrderState == OrderState.Filled;
        }


        private void CalculateTradeTime()
        {

            if ((ToTime(Time[0]) >= 153000 && ToTime(Time[0]) < 210000))
            {
                _canTrade = true;
            }
            else
            {
                _canTrade = false;
            }
        }

        private void AddIndicators()
        {
            if (UseAroon)
            {
                _aroon = Aroon(AroonPeriod);
                AddChartIndicator(_aroon);
            }

            if (UseHMA)
            {
                _fastHMA = HMA(FastHMAPeriod);
                _slowHMA = HMA(SlowHMAPeriod);
                AddChartIndicator(_fastHMA);
                AddChartIndicator(_slowHMA);
            }

            if (ShowHA)
            {
                _ha = HeikenAshi8();
                AddChartIndicator(_ha);
            }
        }
    }
}

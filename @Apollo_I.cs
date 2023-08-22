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
    public class Apollo_I : Strategy
    {
        #region declarations

        private Indicator _aroon;
        private Indicator _stochRsi;


        private double _aroonDown;
        private double _aroonUp;

        private double _stochFast;


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

        private int _entryFastStochValueLong = 29;
        private int _baseStopMarginLong = 8;
        private double _targetRatioLong = 2.0;

        private int _entryFastStochValueShort = 71;
        private int _baseStopMarginShort = 8;
        private double _targetRatioShort = 2.0;

        private int _aroonPeriod = 24;
        private int _stochRsiPeriod = 9;
        private int _fastMAPeriod = 3;
        private int _slowMAPeriod = 3;
        private int _lookBack = 14;

        private bool _useLongs = true;
        private bool _useShorts = true;
        private bool _useAroon = true;

        private int _lotSize1 = 2;
        private int _lotSize2 = 2;

        private int BarNr = 0;  //sprawdzic <-----
        Random rnd = new Random();

        private string status = "Flat";


        #endregion

        #region Longs

        [Display(Name = "Use Longs", GroupName = "LONGS", Order = 0)]
        public bool UseLongs
        {
            get { return _useLongs; }
            set { _useLongs = value; }
        }


        [Display(Name = "Entry Fast Stoch Value", GroupName = "LONGS", Order = 0)]
        public int EntryFastStochValueLong
        {
            get { return _entryFastStochValueLong; }
            set { _entryFastStochValueLong = value; }
        }



        [Display(Name = "Base Stop Margin Long", GroupName = "LONGS", Order = 0)]
        public int BaseStopMarginLong
        {
            get { return _baseStopMarginLong; }
            set { _baseStopMarginLong = value; }
        }

        [Display(Name = "Target to Stop Ratio", GroupName = "SHORTS", Order = 0)]
        public double TargetRatioLong
        {
            get { return _targetRatioLong; }
            set { _targetRatioLong = value; }
        }




        #endregion

        #region Shorts

        [Display(Name = "Use Shorts", GroupName = "SHORTS", Order = 0)]
        public bool UseShorts
        {
            get { return _useShorts; }
            set { _useShorts = value; }
        }


        [Display(Name = "Entry Fast Stoch Value", GroupName = "SHORTS", Order = 0)]
        public int EntryFastStochValueShort
        {
            get { return _entryFastStochValueShort; }
            set { _entryFastStochValueShort = value; }
        }


        [Display(Name = "Base Stop Margin Short", GroupName = "SHORTS", Order = 0)]
        public int BaseStopMarginShort
        {
            get { return _baseStopMarginShort; }
            set { _baseStopMarginShort = value; }
        }

        [Display(Name = "Target to Stop Ratio", GroupName = "SHORTS", Order = 0)]
        public double TargetRatioShort
        {
            get { return _targetRatioShort; }
            set { _targetRatioShort = value; }
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

        #endregion

        #region STOCH

        [Display(Name = "RSI Period ", GroupName = "STOCH", Order = 0)]
        public int StochRsiPeriod
        {
            get { return _stochRsiPeriod; }
            set { _stochRsiPeriod = value; }
        }

        [Display(Name = "FastMAPeriod", GroupName = "STOCH", Order = 0)]
        public int FastMAPeriod
        {
            get { return _fastMAPeriod; }
            set { _fastMAPeriod = value; }
        }

        [Display(Name = "SlowMAPeriod", GroupName = "STOCH", Order = 0)]
        public int SlowMAPeriod
        {
            get { return _slowMAPeriod; }
            set { _slowMAPeriod = value; }
        }

        [Display(Name = "Look Back", GroupName = "STOCH", Order = 0)]
        public int LookBack
        {
            get { return _lookBack; }
            set { _lookBack = value; }
        }
        #endregion


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Apollo I";
                Name = "Apollo I";
                Calculate = Calculate.OnBarClose;

            }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;

                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                AddHeikenAshi("MNQ 09-23", BarsPeriodType.Minute, 1, MarketDataType.Last);
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
            /*
            Print("STOCH SK:");
            Print(StochRSIMod2NT8(9, 3, 3, 14).SK[0]);
            Print("STOCH sD:");
            Print(StochRSIMod2NT8(9, 3, 3, 14).SD[0]);
            */

            if (BarsInProgress == 1)
            {

                _aroonDown = Aroon(BarsArray[1], AroonPeriod).Down[0];
                _aroonUp = Aroon(BarsArray[1], AroonPeriod).Up[0];
                _stochFast = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];

                Print("BAR Summary:");
                Print(Time[0]);
                Print("HAIKEN ASHI!");
                Print("CLOSE" + Close[0]);
                Print("HIGH" + High[0]);
                Print("LOW" + Low[0]);
                Print("OPEN" + Open[0]);
                Print("~~~~~~~~~~~~~~~~~~");
                Print("AROON UP:");
                Print(_aroonUp);
                Print("AROON Down:");
                Print(_aroonDown);
                Print("~~~~~~~~~~~~~~~~~~");
                Print("Long:");
                Print("TARGET PRICE:");
                Print(_longTarget1);
                Print("Stop Loss:");
                Print(_stopLossBaseLong);
                Print("~~~~~~~~~~~~~~~~~~");
                Print("Œhort:");
                Print("TARGET PRICE:");
                Print(_shortTarget1);
                Print("Stop Loss:");
                Print(_stopLossBaseShort);
                Print("*******************");
            }


            if (UseLongs)
            {
                if (LongCondition1())
                {
                    _longOneOrder = EnterLong(LotSize1,  "Long Entry1");
                    _longTwoOrder = EnterLong(LotSize2,  "Long Entry2");
                }


            }
            if (UseShorts)
            {
                if (ShortCondition1())
                {
                    _shortOneOrder = EnterShort(LotSize1, "Short Entry1");
                    _shortTwoOrder = EnterShort(LotSize2, "Short Entry2");
                }

            }

        }



        private bool ShortCondition1()
        {
            _stochFast = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1] ;
            return Position.MarketPosition == MarketPosition.Flat && _stochFast >= EntryFastStochValueShort && previousCandleRed() && IsAroonDowntrend(); //&& Closes[0][0] < _sma[0]; //|| (isDowntrend() && Position.MarketPosition == MarketPosition.Flat && _checkPointShort == true && _canTrade && _rsiEntry[0] <= EntryRsiValueShort - _thresholdShort - 1);
        }


        private bool LongCondition1()
        {
            _stochFast = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
            return Position.MarketPosition == MarketPosition.Flat && _stochFast <= EntryFastStochValueLong && previousCandleGreen() && IsAroonUptrend(); //&& Closes[0][0] > _sma[0]; // || (isUptrend() && Position.MarketPosition == MarketPosition.Flat && _checkPointLong == true &&  _canTrade && _rsiEntry[0] >= EntryRsiValueLong + _thresholdLong + 1 );
        }


        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (OrderFilled(order) && IsLongOrder1(order))
            {
                _longEntryPrice1 = averageFillPrice;
                _stopLossBaseLong = calculateStopLong();
                _longTarget1 = _longEntryPrice1 + ((averageFillPrice - _stopLossBaseLong) * TargetRatioLong);
                SetStopLoss("Long Entry1",CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Long Entry1", CalculationMode.Price, _longTarget1);
                status = "Long 1 Default";
            }

            else if (OrderFilled(order) && IsLongOrder2(order))
            {
                _longTarget1 = _longEntryPrice1 + ((averageFillPrice - _stopLossBaseLong) * TargetRatioLong * 1.5);
                SetStopLoss("Long Entry2", CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Long Entry2", CalculationMode.Price, _longTarget1);
            }


            else if (OrderFilled(order) && IsShortOrder1(order))
            {
                _shortEntryPrice1 = averageFillPrice;
                _stopLossBaseShort = calculateStopShort();
                _shortTarget1 = _shortEntryPrice1 - ((_stopLossBaseShort - averageFillPrice)  * TargetRatioShort);

                SetStopLoss("Short Entry1", CalculationMode.Price, _stopLossBaseShort, false);
                SetProfitTarget("Short Entry1", CalculationMode.Price, _shortTarget1);
                status = "Short 1 Default";
            }
            else if (OrderFilled(order) && IsShortOrder2(order))
            {
                _shortTarget1 = _shortEntryPrice1 - ((_stopLossBaseShort - averageFillPrice) * TargetRatioShort * 1.5);
                SetStopLoss("Short Entry2",CalculationMode.Price, _stopLossBaseShort, false);
                SetProfitTarget("Short Entry2", CalculationMode.Price, _shortTarget1);
            }

        }

        private double calculateStopLong()
        {
            List<double> lows = new List<double> { Lows[0][1], Lows[0][2], Lows[0][3], Lows[0][4]};
            lows.Sort();
            double lowestLow = lows[0];
            double baseStopLoss = lowestLow - BaseStopMarginLong * TickSize;
            Print("~~~~~~~~~");
            Print("LOWEST LOW for Stop Loss:");
            Print(lowestLow);
            Print("Stop Loss Set at:");
            Print(baseStopLoss);
            Print("~~~~~~~~~");

            return baseStopLoss;
        }

        private double calculateStopShort()
        {
            List<double> highs = new List<double> { Highs[0][1], Highs[0][2], Highs[0][3], Highs[0][4] };
            highs.Sort();
            highs.Reverse();
            double highestHigh = highs[0];
            double baseStopLoss = highestHigh + BaseStopMarginShort * TickSize;
            Print("~~~~~~~~~");
            Print("Highest High for Stop Loss:");
            Print(highestHigh);
            Print("Stop Loss Set at:");
            Print(baseStopLoss);
            Print("~~~~~~~~~");

            return baseStopLoss;
        }

        private bool IsLongOrder1(Order order)
        {
            return order == _longOneOrder;
        }

        private bool IsLongOrder2(Order order)
        {
            return order == _longTwoOrder;
        }

        private bool IsAroonUptrend()
        {
            if (UseAroon)
            {
                return Aroon(BarsArray[1], AroonPeriod).Up[0] > Aroon(BarsArray[1], AroonPeriod).Down[0];
            }
            else
            {
                return true;
            }

        }

        private bool IsAroonDowntrend()
        {
            if (UseAroon)
            {
                return Aroon(BarsArray[1], AroonPeriod).Up[0] < Aroon(BarsArray[1], AroonPeriod).Down[0];
            }
            else
            {
                return true;
            }

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

        private bool previousCandleRed()
        {
            return Closes[1][0] < Opens[1][0] && Closes[1][1] > Opens[1][1];
        }

        private bool previousCandleGreen()
        {
            return Closes[1][0] > Opens[1][0] && Closes[1][1] < Opens[1][1];
        }



        private void AddIndicators()
        {

            _aroon = Aroon(BarsArray[1], AroonPeriod);
            AddChartIndicator(_aroon);
            AddChartIndicator(StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack));

        }
    }
}

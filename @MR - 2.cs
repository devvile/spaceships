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
    public class MR2 : Strategy
    {
        #region declarations

        private Indicator _aroon;

        private Indicator _atr;
        private Indicator _stoch;
        private Indicator _fastHma;
        private Indicator _midHma;
        private Indicator _slowHma;
        private double _aroonUp;
        private int _barsToCheck = 60;
        private double _longStopMargin = 8;
        private double _longTargetMargin = 8;
        private double _shortStopMargin = 8;
        private double _shortTargetMargin = 8;
        private bool _canTrade = false;
        private double _longEntryPrice1;
        private double _shortEntryPrice1;
        private int _shortTarget1;
        private int _longTarget1;

        private bool UseRsi = true;

        private int _hmaSlowPeriod = 100;
        private int _hmaMidPeriod = 50;
        private int _hmaFastPeriod = 25;
        private int _maxLossMargin = 50;
        private double _atrFilterValue;
        private int _atrPeriod;

        private Order _longOneOrder;
        private Order _longTwoOrder;
        private Order _longThreeOrder;

        private Order _shortOneOrder;
        private Order _shortTwoOrder;
        private Order _shortThreeOrder;
        #endregion

        #region My Parameters

        private int _entryFastStochValueLong = 19;
        private int _baseStopMarginLong = 8;
        private double _targetRatioLong = 2.0;

        private int _entryFastStochValueShort = 81;
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

        private int _rsiEntryShort = 90;
        private int _rsiEntryLong = 10;

        private int BarNr = 0;  //sprawdzic <-----
        Random rnd = new Random();

        private string status = "Flat";


        #endregion

        [Display(Name = "Aroon Period", GroupName = "Aroon", Order = 0)]
        public int AroonPeriod
        {
            get { return _aroonPeriod; }
            set { _aroonPeriod = value; }
        }



        #region HMA

        [Display(Name = "HMA Slow", GroupName = "HMA", Order = 0)]
        public int HmaSlowPeriod
        {
            get { return _hmaSlowPeriod; }
            set { _hmaSlowPeriod = value; }
        }

        [Display(Name = "HMA Mid ", GroupName = "HMA", Order = 0)]
        public int HmaMidPeriod
        {
            get { return _hmaMidPeriod; }
            set { _hmaMidPeriod = value; }
        }

        [Display(Name = "HMA Fast", GroupName = "HMA", Order = 0)]
        public int HmaFastPeriod
        {
            get { return _hmaFastPeriod; }
            set { _hmaFastPeriod = value; }
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

        #region Longs

        [Display(Name = "Long Stop Margin", GroupName = "LONGS", Order = 0)]
        public double LongStopMargin
        {
            get { return _longStopMargin; }
            set { _longStopMargin = value; }
        }

        [Display(Name = "Long Target Margin", GroupName = "LONGS", Order = 0)]
        public double LongTargetMargin
        {
            get { return _longTargetMargin; }
            set { _longTargetMargin = value; }
        }

        [Display(Name = "Long1  Fixed Target ", GroupName = "LONGS", Order = 0)]
        public int LongTarget1
        {
            get { return _longTarget1; }
            set { _longTarget1 = value; }
        }

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "LONGS", Order = 0)]
        public int RsiEntryLong
        {
            get { return _rsiEntryLong; }
            set { _rsiEntryLong = value; }
        }
        #endregion

        #region Shorts

        [Display(Name = "Short Stop Margin", GroupName = "SHORTS", Order = 0)]
        public double ShortStopMargin
        {
            get { return _shortStopMargin; }
            set { _shortStopMargin = value; }
        }
        [Display(Name = "Stoch Rsi Entry Value", GroupName = "SHORTS", Order = 0)]
        public int RsiEntryShort
        {
            get { return _rsiEntryShort; }
            set { _rsiEntryShort = value; }
        }


        [Display(Name = "Short Target Margin", GroupName = "SHORTS", Order = 0)]
        public double ShortTargetMargin
        {
            get { return _shortTargetMargin; }
            set { _shortTargetMargin = value; }
        }
        [Display(Name = "Short1  Fixed Target ", GroupName = "SHORTS", Order = 0)]
        public int ShortTarget1
        {
            get { return _shortTarget1; }
            set { _shortTarget1 = value; }
        }
        #endregion

        #region Position Management

        [Display(Name = "Bars to Check", GroupName = "Position Management", Order = 0)]
        public int BarsToCheck
        {
            get { return _barsToCheck; }
            set { _barsToCheck = value; }
        }

        [Display(Name = "Max Stop Loss Margin", GroupName = "Position Management", Order = 0)]
        public int MaxLossMargin
        {
            get { return _maxLossMargin; }
            set { _maxLossMargin = value; }
        }

        #endregion

        #region Filters

        [Display(Name = "Atr period", GroupName = "Filters", Order = 0)]
            public int AtrPeriod
            {
                get { return _atrPeriod; }
                set { _atrPeriod = value; }
            }

            [Display(Name = "Atr filter value", GroupName = "Filters", Order = 0)]
            public double AtrFilterValue
            {
                get { return _atrFilterValue; }
                set { _atrFilterValue = value; }
            }

        #endregion


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Sandbox";
                Name = "MR 2 Mean Reversion";
                Calculate = Calculate.OnBarClose;
                BarsRequiredToTrade = 60;

            }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;

                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                BarsRequiredToTrade = BarsToCheck;
                AddDataSeries(BarsPeriodType.Minute, 1);
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
            if (CurrentBar < BarsRequiredToTrade || CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade)
                return;
            CalculateTradeTime();

            if (Position.MarketPosition == MarketPosition.Flat)
            {
                status = "Flat";

            }

            if (BarsInProgress == 1)

            { 

                if (Position.MarketPosition == MarketPosition.Flat && stochRsiEntry(RsiEntryLong, "Long") && previousCandleGreen() && meanReversionConditions() && _atr[0] >= AtrFilterValue)
                {
                    _longOneOrder = EnterLong(1, "Long1");
                    _longTwoOrder = EnterLong(1, "Long2");

                }
                else if (Position.MarketPosition == MarketPosition.Flat && stochRsiEntry(RsiEntryShort, "Short") && previousCandleRed() && meanReversionConditions() && _atr[0] >= AtrFilterValue)
                {
                    _shortOneOrder= EnterShort(1, "Short1");
                    _shortTwoOrder = EnterShort(1, "Short2");

                }

            }

        }

        private bool meanReversionConditions()
        {
            return _canTrade && Aroonize() && HmaFilter();
        }

        private bool AroonUp()
        {
            return (Aroon(BarsArray[0], AroonPeriod).Up[0] > 70);
        }

        private bool AroonDown()
        {
            return (Aroon(BarsArray[0], AroonPeriod).Down[0] > 70);
        }

        private bool Aroonize()
        {

            return (Aroon(BarsArray[0], AroonPeriod).Up[0] < 70 && Aroon(BarsArray[0], AroonPeriod).Down[0] < 70);// && (Aroon(BarsArray[0], AroonPeriod).Up[0] > 10 && Aroon(BarsArray[0], AroonPeriod).Down[0] > 10);
        }

        private bool HmaFilter()
        {

            return (_fastHma[0] > _midHma[0] && _midHma[0] < _slowHma[0]) || (_fastHma[0] < _midHma[0] && _midHma[0] > _slowHma[0]) || (_fastHma[0] < _slowHma[0] && _midHma[0] > _slowHma[0]) || (_fastHma[0] < _slowHma[0] && _midHma[0] > _slowHma[0]);
        }



        private bool previousCandleRed()
        {
            return HeikenAshi8(BarsArray[1]).HAOpen[0] > HeikenAshi8(BarsArray[1]).HAClose[0] && HeikenAshi8(BarsArray[1]).HAOpen[1] < HeikenAshi8(BarsArray[1]).HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8(BarsArray[1]).HAOpen[0] < HeikenAshi8(BarsArray[1]).HAClose[0] && HeikenAshi8(BarsArray[1]).HAOpen[1] > HeikenAshi8(BarsArray[1]).HAClose[1];
        }

        private double calculateStopLong()
        {
            List<double> lows = new List<double> { };
            int i = 0;
            while (i < BarsToCheck)
            {
                lows.Add(Lows[1][i]);

                i++; // increment
            }
            lows.Sort();
            //        double highestHigh = highs[0];

            double dynamicStopLoss = lows[0] - LongStopMargin * TickSize;
            double maxStopLoss = _longEntryPrice1 - MaxLossMargin * TickSize;

            if (dynamicStopLoss < maxStopLoss)
            {
                return maxStopLoss;
            }
            else
            {
                return dynamicStopLoss;
            }
        }

        private double calculateTargetLong()
        {
            List<double> highs = new List<double> { };
            int i = 0;
            while (i < BarsToCheck)
            {
                highs.Add(Highs[1][i]);

                i++; // increment
            }
            highs.Sort();
            highs.Reverse();
            double highestHigh = highs[0];

            return highestHigh + LongTargetMargin * TickSize;
        }

        private double calculateStopShort()
        {
            List<double> highs = new List<double> { };
            int i = 0;
            while (i < BarsToCheck)
            {
                highs.Add(Highs[1][i]);

                i++; // increment
            }
            highs.Sort();
            highs.Reverse();
            double highestHigh = highs[0];

            double dynamicStopLoss = highestHigh + ShortStopMargin * TickSize;
            double maxStopLoss = _shortEntryPrice1 + MaxLossMargin * TickSize;

            if (dynamicStopLoss < maxStopLoss)
            {
                return dynamicStopLoss;
            }
            else
            {
                return maxStopLoss;
            }
        }

        private double calculateTargetShort()
        {
            List<double> lows = new List<double> { };
            int i = 0;
            while (i < BarsToCheck)
            {
                lows.Add(Lows[1][i]);

                i++; // increment
            }
            lows.Sort();
            //        double highestHigh = highs[0];

            return lows[0] - ShortTargetMargin * TickSize;
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


        private bool stochRsiEntry(int entryValue, string positionType)
        {
            if (UseRsi)
            {
                double _stochFast = StochRSIMod2NT8(BarsArray[1], StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
                if (positionType == "Long")
                {
                    return _stochFast <= entryValue;
                }
                else if (positionType == "Short")
                {
                    return _stochFast >= entryValue;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

        }
        private void AddIndicators()
        {
            _atr = ATR(BarsArray[0],AtrPeriod);
            _stoch = StochRSIMod2NT8(BarsArray[1], StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack);
            _aroon = Aroon(BarsArray[0], AroonPeriod);
            _fastHma = HMA(BarsArray[0], HmaFastPeriod);
            _midHma = HMA(BarsArray[0], HmaMidPeriod);
            _slowHma = HMA(BarsArray[0], HmaSlowPeriod);
            AddChartIndicator(_stoch);
            AddChartIndicator(_aroon);
            AddChartIndicator(_fastHma);
            AddChartIndicator(_midHma);
            AddChartIndicator(_slowHma);

        }

        private double calculateTargetSmallLong()
        {
            double normalTarget = calculateTargetLong();
            double smallTarget = _longEntryPrice1 + TickSize * LongTarget1;

            if (normalTarget < smallTarget)
            {
                return normalTarget;
            }
            else
            {
              return  smallTarget;
            }
        }

        private double calculateTargetSmallShort()
        {
            double normalTarget = calculateTargetShort();
            double smallTarget = _shortEntryPrice1 - TickSize * ShortTarget1;

            if (normalTarget > smallTarget)
            {
                return normalTarget;
            }
            else
            {
                return smallTarget;
            }
        }
        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
           /* if  (order.Name == "Profit target" && OrderFilled(order) )
            {
                double breakEvenPrice = 0;
                if(Position.MarketPosition == MarketPosition.Long)
                {
                    breakEvenPrice = _longEntryPrice1 - LongStopMargin;
                }else if(Position.MarketPosition == MarketPosition.Short)
                {
                    breakEvenPrice = _shortEntryPrice1 + ShortStopMargin;
                }
               SetStopLoss(CalculationMode.Price, breakEvenPrice);
            } */
            if (OrderFilled(order) && IsLongOrder1(order))
            {
                _longEntryPrice1 = averageFillPrice;
                SetStopLoss(CalculationMode.Price, calculateStopLong());
                SetProfitTarget("Long1", CalculationMode.Price, calculateTargetSmallLong());

            }
            else if (OrderFilled(order) && IsLongOrder2(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopLong());
                SetProfitTarget("Long2", CalculationMode.Price, calculateTargetLong());
            }
            else if (OrderFilled(order) && IsLongOrder3(order))
            {
     //           SetProfitTarget("Long3", CalculationMode.Ticks, ProfitTargetLong3);
            }

            else if (OrderFilled(order) && IsShortOrder1(order))
            {
                   _shortEntryPrice1 = averageFillPrice;
                   SetStopLoss(CalculationMode.Price, calculateStopShort());
                SetProfitTarget("Short1", CalculationMode.Price, calculateTargetSmallShort());
            }
            else if (OrderFilled(order) && IsShortOrder2(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopShort());
                SetProfitTarget("Short2", CalculationMode.Price, calculateTargetShort());
            }
            else if (OrderFilled(order) && IsShortOrder3(order))
            {
   //             SetProfitTarget("Short3", CalculationMode.Ticks, ProfitTargetShort3);
            }

            //   MonitorStopProfit(order, limitPrice, stopPrice);
            //        MoveStopToBreakeven(order);

        }

        #region Orders Conditions

        private bool IsLongOrder1(Order order)
        {
            return order == _longOneOrder;
        }

        private bool IsLongOrder2(Order order)
        {
            return order == _longTwoOrder;
        }

        private bool IsLongOrder3(Order order)
        {
            return order == _longThreeOrder;
        }


        private bool IsShortOrder1(Order order)
        {
            return order == _shortOneOrder;
        }

        private bool IsShortOrder2(Order order)
        {
            return order == _shortTwoOrder;
        }

        private bool IsShortOrder3(Order order)
        {
            return order == _shortThreeOrder;
        }


        private bool OrderFilled(Order order)
        {
            return order.OrderState == OrderState.Filled;
        }

        #endregion
    }


}
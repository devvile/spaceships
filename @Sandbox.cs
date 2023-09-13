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
    public class PriceActionMan : Strategy
    {
        #region declarations

        private Indicator _aroon;
        private Indicator _stoch;
        private Indicator _fastHma;
        private Indicator _midHma;
        private Indicator _slowHma;
        private Indicator _atr;


        private double _longEntryPrice;
        private double _stopLossBaseLong;
        private int _profitTargetLong1;
        private int _profitTargetLong2;
        private int _profitTargetLong3;
        private Order _longOneOrder;
        private Order _longTwoOrder;
        private Order _longThreeOrder;
        private int _longStopMargin;
        private double _atrFilterValue;
        private int _maxLossMargin;
        private double maxLossStop;

        private double _shortEntryPrice;
        private double _stopLossBaseShort;
        private int _profitTargetShort1;
        private int _profitTargetShort2;
        private int _profitTargetShort3;
        private Order _shortOneOrder;
        private Order _shortTwoOrder;
        private Order _shortThreeOrder;
        private int _shortStopMargin;


        private bool _useATR;
        private int _atrPeriod;
        private double _aroonUp;
        private int _barsToCheck = 60;
        private bool _canTrade = false;
        private bool UseRsi = true;

        private double _stopLossPrice;
        private int _hmaSlowPeriod = 100;
        private int _hmaMidPeriod = 50;
        private int _hmaFastPeriod = 25;
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
        private string _trend;

        private int _lotSize1 = 1;
        private int _lotSize2 = 1;
        private int _lotSize3 = 1;

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

        #region ATR

        [Display(Name = "ATR Filter", GroupName = "ATR", Order = 0)]
        public bool UseATR
        {
            get { return _useATR; }
            set { _useATR = value; }
        }

        [Display(Name = "ATR Period", GroupName = "ATR", Order = 0)]
        public int AtrPeriod
        {
            get { return _atrPeriod; }
            set { _atrPeriod = value; }
        }

        [Display(Name = "ATR Filter Value", GroupName = "ATR", Order = 0)]
        public double AtrFilterValue
        {
            get { return _atrFilterValue; }
            set { _atrFilterValue = value; }
        }
        #endregion

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

        [Display(Name = "Long Stop Margin", GroupName = "Long", Order = 0)]
        public int LongStopMargin
        {
            get { return _longStopMargin; }
            set { _longStopMargin = value; }
        }

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Long", Order = 0)]
        public int RsiEntryLong
        {
            get { return _rsiEntryLong; }
            set { _rsiEntryLong = value; }
        }

        [Display(Name = "Profit Target 1", GroupName = "Long", Order = 0)]
        public int ProfitTargetLong1
        {
            get { return _profitTargetLong1; }
            set { _profitTargetLong1 = value; }
        }

        [Display(Name = "Profit Target 2", GroupName = "Long", Order = 0)]
        public int ProfitTargetLong2
        {
            get { return _profitTargetLong2; }
            set { _profitTargetLong2 = value; }
        }

        [Display(Name = "Profit Target 3", GroupName = "Long", Order = 0)]
        public int ProfitTargetLong3
        {
            get { return _profitTargetLong3; }
            set { _profitTargetLong3 = value; }
        }
        #endregion

        #region Shorts

        [Display(Name = "Short Stop Margin", GroupName = "Short", Order = 0)]
        public int ShortStopMargin
        {
            get { return _shortStopMargin; }
            set { _shortStopMargin = value; }
        }
        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Short", Order = 0)]
        public int RsiEntryShort
        {
            get { return _rsiEntryShort; }
            set { _rsiEntryShort = value; }
        }

        [Display(Name = "Profit Target 1", GroupName = "Short", Order = 0)]
        public int ProfitTargetShort1
        {
            get { return _profitTargetShort1; }
            set { _profitTargetShort1 = value; }
        }

        [Display(Name = "Profit Target 2", GroupName = "Short", Order = 0)]
        public int ProfitTargetShort2
        {
            get { return _profitTargetShort2; }
            set { _profitTargetShort2 = value; }
        }

        [Display(Name = "Profit Target 3", GroupName = "Short", Order = 0)]
        public int ProfitTargetShort3
        {
            get { return _profitTargetShort3; }
            set { _profitTargetShort3 = value; }
        }
        #endregion

        #region Position Management

        [Display(Name = "Bars to Check", GroupName = "Position Management", Order = 0)]
        public int BarsToCheck
        {
            get { return _barsToCheck; }
            set { _barsToCheck = value; }
        }

        [Display(Name = "Max Loss Margin", GroupName = "Max Loss Margin", Order = 0)]
        public int MaxLossMargin
        {
            get { return _maxLossMargin; }
            set { _maxLossMargin = value; }
        }

        [Display(Name = "Entry Size 1", GroupName = "Position Management", Order = 0)]
        public int LotSize1
        {
            get { return _lotSize1; }
            set { _lotSize1 = value; }
        }

        [Display(Name = "Entry Size 2", GroupName = "Position Management", Order = 0)]
        public int LotSize2
        {
            get { return _lotSize2; }
            set { _lotSize2 = value; }
        }

        [Display(Name = "Entry Size 3", GroupName = "Position Management", Order = 0)]
        public int LotSize3
        {
            get { return _lotSize3; }
            set { _lotSize3 = value; }
        }


        #endregion



        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Sandbox";
                Name = "Sandbox";
                Calculate = Calculate.OnBarClose;
                BarsRequiredToTrade = 60;
                _longStopMargin = 8;
                _shortStopMargin = 8;

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

            if (BarsInProgress == 1) {
                _trend = CalculateTrend();
                if (_canTrade && applyFilters())
                {
                    if(_trend == "Uptrend" && TriggerConditions(_trend) && noPosition())
                    {
                        goLong();
                    }else if(_trend == "Downtrend" && TriggerConditions(_trend) && noPosition())
                    {
                        goShort();
                    }
                    else if (_trend == "Mean Reversion" && TriggerConditions(_trend) && noPosition())
                    {
            //            goBoth();
                    }
                    else
                    {
                        // IGNORE?
                    }
                }

            }

        }

        #region Entry

        private void goLong()
        {
            _longOneOrder = EnterLong(LotSize1, "Long1");
            _longTwoOrder = EnterLong(LotSize2, "Long2");
            _longThreeOrder = EnterLong(LotSize3, "Long3");
        }

        private void goShort()
        {
  //         _shortOneOrder = EnterShort(LotSize1, "Short1");
    //       _shortTwoOrder = EnterShort(LotSize2, "Short2");
    //        _shortThreeOrder = EnterShort(LotSize3, "Short3");
        }

        #endregion

        private bool noPosition()
        {
            return Position.MarketPosition == MarketPosition.Flat;
        }

        private bool applyFilters()
        {
            if (UseATR)
            {
                if (_atr[0] >= AtrFilterValue)
                {
                    return true;
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

        #region Triggers
        private bool TriggerConditions(string trend)
        {
            if(trend == "Uptrend")
            {
                return LongTrigger();
            }
            else if(trend == "Downtrend")
            {
                return ShortTrigger();
            }
            else if(trend == "Mean Reversion")
            {
                return ShortTrigger() || LongTrigger();
            }
            else
            {
                return false;
            }
        }

        private bool LongTrigger()
        {
            return stochRsiEntry(RsiEntryLong, "Long") && previousCandleGreen();
        }

        private bool ShortTrigger()
        {
           return  stochRsiEntry(RsiEntryShort, "Short") && previousCandleRed();
        }

        #endregion

        private string CalculateTrend()
        {
            if (IsAroonUp() && IsMALong())
            {
                return "Uptrend";
            }
            else if (IsAroonMeanReversion() && IsMAMeanReversion())
            {
                return "Mean Reversion";
            } else if (IsAroonDown() && IsMAShort())
            {
                return "Downtrend";
            }
            else { return "Not Clear"; }
        
         }

        #region Aroon
        private bool IsAroonUp()
        {
            return (Aroon(BarsArray[0], AroonPeriod).Up[0] > 70);
        }

        private bool IsAroonDown()
        {
            return (Aroon(BarsArray[0], AroonPeriod).Down[0] > 70);
        }

        private bool IsAroonMeanReversion()
        {

            return (Aroon(BarsArray[0], AroonPeriod).Up[0] < 70 && Aroon(BarsArray[0], AroonPeriod).Down[0] < 70);// && (Aroon(BarsArray[0], AroonPeriod).Up[0] > 10 && Aroon(BarsArray[0], AroonPeriod).Down[0] > 10);
        }

        #endregion

        #region MA

        private bool IsMAMeanReversion()
        {

            return (_fastHma[0] > _midHma[0] && _midHma[0] < _slowHma[0]) || (_fastHma[0] < _midHma[0] && _midHma[0] > _slowHma[0]) || (_fastHma[0] < _slowHma[0] && _midHma[0] > _slowHma[0]) || (_fastHma[0] < _slowHma[0] && _midHma[0] > _slowHma[0]);
        }

        private bool IsMALong()
        {
            return (_fastHma[0] > _midHma[0] && _midHma[0] > _slowHma[0]);
        }

        private bool IsMAShort()
        {
            return (_fastHma[0] < _midHma[0] && _midHma[0] < _slowHma[0]);
        }

        #endregion

        #region Ichimoku
        private bool previousCandleRed()
        {
            return HeikenAshi8(BarsArray[1]).HAOpen[0] > HeikenAshi8(BarsArray[1]).HAClose[0] && HeikenAshi8(BarsArray[1]).HAOpen[1] < HeikenAshi8(BarsArray[1]).HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8(BarsArray[1]).HAOpen[0] < HeikenAshi8(BarsArray[1]).HAClose[0] && HeikenAshi8(BarsArray[1]).HAOpen[1] > HeikenAshi8(BarsArray[1]).HAClose[1];
        }
        #endregion


        #region Stop Calculation
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

            double dynamicStopLoss = lows[0] - LongStopMargin * TickSize;
            double maxStopLoss = _longEntryPrice - MaxLossMargin * TickSize;

            if (dynamicStopLoss < maxStopLoss)
            {
                return maxStopLoss;
            }
            else
            {
                return dynamicStopLoss;
            }
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
            double maxStopLoss = _shortEntryPrice + MaxLossMargin * TickSize;

            if (dynamicStopLoss > maxStopLoss)
            {
                return maxStopLoss;
            }
            else
            {
                return dynamicStopLoss;
            }
        }
        #endregion

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
               double  _stochFast = StochRSIMod2NT8(BarsArray[1],StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
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

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (OrderFilled(order) && IsLongOrder1(order))
            {
                _longEntryPrice = averageFillPrice;
                _stopLossPrice = calculateStopLong();
                SetProfitTarget("Long1", CalculationMode.Ticks, ProfitTargetLong1);
            }
            else if (OrderFilled(order) && IsLongOrder2(order))
            {
                SetProfitTarget("Long2", CalculationMode.Ticks, ProfitTargetLong2);
            }
            else if (OrderFilled(order) && IsLongOrder3(order))
            {
                SetProfitTarget("Long3", CalculationMode.Ticks, ProfitTargetLong3);
            }

            else if (OrderFilled(order) && IsShortOrder1(order))
            {
                _shortEntryPrice = averageFillPrice;
                _stopLossPrice = calculateStopShort();
                SetProfitTarget("Short1", CalculationMode.Ticks, ProfitTargetShort1);
            }
            else if (OrderFilled(order) && IsShortOrder2(order))
            {
                SetProfitTarget("Short2", CalculationMode.Ticks, ProfitTargetShort2);
            }
            else if (OrderFilled(order) && IsShortOrder3(order))
            {
                SetProfitTarget("Short3", CalculationMode.Ticks, ProfitTargetShort3);
            }

            //   MonitorStopProfit(order, limitPrice, stopPrice);
            //        MoveStopToBreakeven(order);

        }

        private void AddIndicators()
        {
            _stoch = StochRSIMod2NT8(BarsArray[1],StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack);
            _aroon = Aroon(BarsArray[0],AroonPeriod);
            _fastHma = HMA(BarsArray[0],HmaFastPeriod);
            _midHma = HMA(BarsArray[0],HmaMidPeriod);
            _slowHma = HMA(BarsArray[0],HmaSlowPeriod);
            AddChartIndicator(_stoch);
            AddChartIndicator(_aroon);
            AddChartIndicator(_fastHma);
            AddChartIndicator(_midHma);
            AddChartIndicator(_slowHma);
            if (UseATR)
            {
                _atr = ATR(BarsArray[0],AtrPeriod);
                AddChartIndicator(_atr);
            }

        }
    }
}

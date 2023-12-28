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
    public class Apollo_VII : Strategy
    {
        #region Declarations
        private int LotSize1;
        private int LotSize2;
        private int LotSize3;
        private int _userLotSize1;
        private int _userLotSize2;
        private int _userLotSize3;
        private Indicator _ha;
        private Indicator _aroon;
        private Indicator _stoch;
        private Indicator _iczimoku;
        private Indicator _atr;
        private Indicator _Vix;
        private double acummulated;
        private double _maxLoss;
        private double _maxProfit;

        Random rnd = new Random();
        private int _aroonPeriod;
        private bool _useLongs;
        private bool _useShorts;
        private double _stochFast;

        private int _atrPeriod;

        private double stopLossPrice;

        private double _longEntryPrice1;
        private double _stopLossBaseLong;
        private double _profitTargetLong1;
        private double _profitTargetLong2;
        private double _profitTargetLong3;
        private Order _longOneOrder;
        private Order _longTwoOrder;
        private Order _longThreeOrder;
        private int _longStopMargin;
        private double _atrFilterValue;
        private int _maxLossMargin;
        private double maxLossStop;
        private int _barsToCheck;
        private int _extraEntryRsiLong;
        private int _extraEntryRsiShort;
        private double _atrValue;

        private double _shortEntryPrice1;
        private double _stopLossBaseShort;
        private double _profitTargetShort1;
        private double _profitTargetShort2;
        private double _profitTargetShort3;
        private Order _shortOneOrder;
        private Order _shortTwoOrder;
        private Order _shortThreeOrder;
        private int _shortStopMargin;
        private List<Order> stopLossOrders;
        private List<Order> profitTargetOrders;

        private int _extraSize;
        private bool _cutLoss = true;
        private bool _cutProfit = true;
        private int _trailThresholdShort;
        private int _trailThresholdShort2;
        private int _trail2LevelShort;

        private int _trailThresholdLong;
        private int _trailThresholdLong2;
        private int _trail2LevelLong;
        private double _dailyProfitLimit = 800;
        private double _dailyLossLimit = -800;
        private double _shortAtrRatio = 0.8;
        private double _longAtrRatio = 0.8;
        #endregion

        #region Parameters

        private int _rsiEntryShort;
        private int _rsiEntryLong;
        private double _vixFilterValue;

        private int _stochRsiPeriod;
        private int _fastMAPeriod;
        private int _slowMAPeriod;
        private int _lookBack;
        private string status = "Flat";
        private bool _canTrade = false;
        private string signal = "No Signal";

        //   private int _minuteIndex =0 ;
        #endregion


        #region Config

        [Display(Name = "Bars to Check", GroupName = "Position Management", Order = 0)]
        public int BarsToCheck
        {
            get { return _barsToCheck; }
            set { _barsToCheck = value; }
        }

        [Display(Name = "Aroon Period", GroupName = "Config", Order = 4)]
        public int AroonPeriod
        {
            get { return _aroonPeriod; }
            set { _aroonPeriod = value; }
        }

        #endregion


        #region Position Management

        [Display(Name = "Entry Size 1", GroupName = "Position Management", Order = 0)]
        public int UserLotSize1
        {
            get { return _userLotSize1; }
            set { _userLotSize1 = value; }
        }

        [Display(Name = "Entry Size 2", GroupName = "Position Management", Order = 0)]
        public int UserLotSize2
        {
            get { return _userLotSize2; }
            set { _userLotSize2 = value; }
        }

        [Display(Name = "Entry Size 3", GroupName = "Position Management", Order = 0)]
        public int UserLotSize3
        {
            get { return _userLotSize3; }
            set { _userLotSize3 = value; }
        }

        [Display(Name = "Daily Profit Limit", GroupName = "Position Management", Order = 0)]
        public double DailyProfitLimit
        {
            get { return _dailyProfitLimit; }
            set { _dailyProfitLimit = value; }
        }

        [Display(Name = "Daily Loss Limit", GroupName = "Position Management", Order = 0)]
        public double DailyLossLimit
        {
            get { return _dailyLossLimit; }
            set { _dailyLossLimit = value; }
        }

        [Display(Name = "Cut Loss", GroupName = "Position Management", Order = 0)]
        public bool cutLoss
        {
            get { return _cutLoss; }
            set { _cutLoss = value; }
        }

        [Display(Name = "Cut Profit", GroupName = "Position Management", Order = 0)]
        public bool cutProfit
        {
            get { return _cutProfit; }
            set { _cutProfit = value; }
        }



        [Display(Name = "Emergency stop margin", GroupName = "Position Management", Order = 0)]
        public int MaxLossMargin
        {
            get { return _maxLossMargin; }
            set { _maxLossMargin = value; }
        }

        #endregion

        #region Long

        [Display(Name = "Use Longs", GroupName = "Long", Order = 0)]
        public bool UseLongs
        {
            get { return _useLongs; }
            set { _useLongs = value; }
        }

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Long", Order = 1)]
        public int RsiEntryLong
        {
            get { return _rsiEntryLong; }
            set { _rsiEntryLong = value; }
        }

        [Display(Name = "Extra Entry Stoch Rsi Long Value", GroupName = "Long", Order = 1)]
        public int ExtraEntryRsiLong
        {
            get { return _extraEntryRsiLong; }
            set { _extraEntryRsiLong = value; }
        }

        [Display(Name = "ATR TARGET RATIO LONG", GroupName = "Long", Order = 2)]
        public double LongAtrRatio
        {
            get { return _longAtrRatio; }
            set { _longAtrRatio = value; }
        }


        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Profit Runner Long", GroupName = "Long", Order = 2)]
        public double ProfitTargetLong3
        {
            get { return _profitTargetLong3; }
            set { _profitTargetLong3 = value; }
        }

        [Display(Name = "Dynamic Stop Margin", GroupName = "Long", Order = 2)]
        public int LongStopMargin
        {
            get { return _longStopMargin; }
            set { _longStopMargin = value; }
        }

        [Display(Name = "Breakeven Trail Threshold", GroupName = "Long", Order = 3)]
        public int TrailThresholdLong
        {
            get { return _trailThresholdLong; }
            set { _trailThresholdLong = value; }
        }

        [Display(Name = "Final Trail Threshold", GroupName = "Long", Order = 4)]
        public int TrailThresholdLong2
        {
            get { return _trailThresholdLong2; }
            set { _trailThresholdLong2 = value; }
        }

        [Display(Name = "Final Trail Level Long", GroupName = "Long", Order = 5)]
        public int Trail2LevelLong
        {
            get { return _trail2LevelLong; }
            set { _trail2LevelLong = value; }
        }


        #endregion

        #region Short

        [Display(Name = "Use Shorts", GroupName = "Short", Order = 0)]
        public bool UseShorts
        {
            get { return _useShorts; }
            set { _useShorts = value; }
        }

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Short", Order = 1)]
        public int RsiEntryShort
        {
            get { return _rsiEntryShort; }
            set { _rsiEntryShort = value; }
        }

        [Display(Name = "Extra Entry Stoch Rsi Short Value", GroupName = "Short", Order = 1)]
        public int ExtraEntryRsiShort
        {
            get { return _extraEntryRsiShort; }
            set { _extraEntryRsiShort = value; }
        }

        [Display(Name = "Profit target Short", GroupName = "Short", Order = 2)]
        public double ProfitTargetShort1
        {
            get { return _profitTargetShort1; }
            set { _profitTargetShort1 = value; }
        }


        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Profit Runner Short ", GroupName = "Short", Order = 2)]
        public double ProfitTargetShort3
        {
            get { return _profitTargetShort3; }
            set { _profitTargetShort3 = value; }
        }

        [Display(Name = "Dynamic Stop Margin", GroupName = "Short", Order = 2)]
        public int ShortStopMargin
        {
            get { return _shortStopMargin; }
            set { _shortStopMargin = value; }
        }

        [Display(Name = "Breakeven Trail Threshold", GroupName = "Short", Order = 3)]
        public int TrailThresholdShort
        {
            get { return _trailThresholdShort; }
            set { _trailThresholdShort = value; }
        }

        [Display(Name = "Final Trail Threshold", GroupName = "Short", Order = 4)]
        public int TrailThresholdShort2
        {
            get { return _trailThresholdShort2; }
            set { _trailThresholdShort2 = value; }
        }

        [Display(Name = "Final Trail Level Short", GroupName = "Short", Order = 5)]
        public int Trail2LevelShort
        {
            get { return _trail2LevelShort; }
            set { _trail2LevelShort = value; }
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

        #region Filters

        [Display(Name = "ATR Period", GroupName = "Filter", Order = 0)]
        public int AtrPeriod
        {
            get { return _atrPeriod; }
            set { _atrPeriod = value; }
        }

        [Display(Name = "ATR Filter Value", GroupName = "Filter", Order = 0)]
        public double AtrFilterValue
        {
            get { return _atrFilterValue; }
            set { _atrFilterValue = value; }
        }

        [Display(Name = "Vix Filter Value", GroupName = "Filter", Order = 0)]
        public double VixFilterValue
        {
            get { return _vixFilterValue; }
            set { _vixFilterValue = value; }
        }
        #endregion


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Apollo VII";
                Name = "Apollo VII  atr Target";
                Calculate = Calculate.OnBarClose;
                _userLotSize1 = 1;
                _userLotSize2 = 2;
                _userLotSize3 = 1;
                _profitTargetLong3 = 80;
                _longStopMargin = 24;
                _shortStopMargin = 20;
                _profitTargetShort3 = 90;
                _stochRsiPeriod = 14;
                _fastMAPeriod = 3;
                _slowMAPeriod = 3;
                _lookBack = 14;
                _atrPeriod = 100;
                _atrFilterValue = 1.75;
                _maxLossMargin = 75;
                _barsToCheck = 80;
                _useLongs = true;
                _useShorts = true;
                _trailThresholdShort = 60;
                _trailThresholdShort2 = 72;
                _trailThresholdLong = 60;
                _trailThresholdLong2 = 70;
                _aroonPeriod = 10;
                _rsiEntryLong = 10;
                _rsiEntryShort = 90;
                IsInstantiatedOnEachOptimizationIteration = false;


            }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;
                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                profitTargetOrders = new List<Order>();
                stopLossOrders = new List<Order>();

                AddDataSeries(BarsPeriodType.Minute, 4);
                AddDataSeries(BarsPeriodType.Minute, 16);
                AddDataSeries(BarsPeriodType.Day, 1);
                AddDataSeries(BarsPeriodType.Week, 1);

            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();
                addIndicators();
                Calculate = Calculate.OnBarClose;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade || CurrentBars[2] < BarsRequiredToTrade || CurrentBars[3] < 10 || CurrentBars[4] < 1)
                return;

            CalculateTradeTime();


            if (_canTrade && BarsInProgress == 0 && _Vix[0] >VixFilterValue)
            {
                _atrValue = _atr[0];
                Trail();
                AdjustStop();
                IchimokuMode();

            }

            if(BarsInProgress == 3)
            {

            }
        }

        private void Trail()
        {
            if (Close[0] >= Position.AveragePrice + (TrailThresholdLong * TickSize) && Position.MarketPosition == MarketPosition.Long && status!= "Breakeven2 Short" && status != "Breakeven2 Long")
            {
                status = "Breakeven";
            }
            else if (Close[0] <= Position.AveragePrice - (TrailThresholdShort * TickSize) && Position.MarketPosition == MarketPosition.Short && status != "Breakeven2 Short" && status != "Breakeven2 Long")
            {
                status = "Breakeven";
            }
            else if (Close[0] <= Position.AveragePrice - (TrailThresholdShort2 * TickSize) && Position.MarketPosition == MarketPosition.Short)
            {
                status = "Breakeven2 Short";
            }
            else if (Close[0] >= Position.AveragePrice + (TrailThresholdLong2 * TickSize) && Position.MarketPosition == MarketPosition.Long)
            {
                status = "Breakeven2 Long";
            }
        }

        private void IchimokuMode()
        {

            signal = calculateSignal();
            Trail();

            if (signal != "No Singal" && applyFilters())
            {
                if (UseLongs && signal != "Strong Short" && signal != "Weak Short" && noPositions() && previousCandleGreen() && stochRsiEntry(RsiEntryLong, "Long"))
                {
                    if (signal == "Strong Long")
                    {
                        setUserPoisitonSizes();
                        _longOneOrder = EnterLong(LotSize1, "Long1");
                        _longTwoOrder = EnterLong(LotSize2, "Long2");
                        _longThreeOrder = EnterLong(LotSize3, "Long3");
                        ShowEntryDetails(signal);

                    }
                    else if (signal == "Weak Long")
                    {
                       setSmallPositionSizes();
                       _longOneOrder = EnterLong(LotSize1, "Long1");
                       _longTwoOrder = EnterLong(LotSize2, "Long2");
                        //                    _longThreeOrder = EnterLong(LotSize2, "Long3");

                    }

                }
                else if (UseShorts && signal != "Strong Long" && signal != "Weak Long" && noPositions() && previousCandleRed() && stochRsiEntry(RsiEntryShort, "Short"))
                {
                    if (signal == "Strong Short")
                    {

                            setUserPoisitonSizes();
                            _shortOneOrder = EnterShort(LotSize1, "Short1");
                            _shortTwoOrder = EnterShort(LotSize2, "Short2");
                            _shortThreeOrder = EnterShort(LotSize3, "Short3");
                            ShowEntryDetails(signal);
                    }
                    else if ( signal == "Weak Short")
                    {

                            setSmallPositionSizes();
                           _shortOneOrder = EnterShort(LotSize1, "Short1");
                           _shortTwoOrder = EnterShort(LotSize2, "Short2");
                            //                   _shortThreeOrder = EnterShort(LotSize3, "Short3");
                            ShowEntryDetails(signal);

                    }
                }
            }
            if (Position.MarketPosition == MarketPosition.Short && stochRsiExtra( ExtraEntryRsiShort, "Short") && status !="Short Extra" && status != "Breakeven" && status != "Breakeven2 Short")
            {
            //    _shortTwoOrder = EnterShort(LotSize2, "Short2");
            }
            else if (Position.MarketPosition == MarketPosition.Long && stochRsiExtra(ExtraEntryRsiLong, "Long") && status != "Long Extra"  && status != "Breakeven" && status != "Breakeven2 Long")
            {
           //    _longTwoOrder = EnterLong(LotSize2, "Long2");
            }
        }


    private void ExitOnMaxProfitLoss()
        {
            if (Bars.IsFirstBarOfSession)
            {
                acummulated = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
            }

            if (SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit - acummulated > DailyProfitLimit || SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit - acummulated < DailyLossLimit)
            {
                _canTrade = false;
            }
            if (Position.MarketPosition != MarketPosition.Flat)
            {
                if (cutLoss && SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit - acummulated + Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]) <= DailyLossLimit || cutProfit && SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit - acummulated + Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]) >= DailyProfitLimit)
                {
                    ExitLong();
                    ExitShort();
                    _canTrade = false;
                }
            }
        }

        private void AdjustStop()
        {
            if (noPositions())
            {
                status = "Flat";
                SetStopLoss(CalculationMode.Ticks, 50);
            }
            else if (status == "Short Default")
            {
                SetStopLoss(CalculationMode.Price, stopLossPrice);
            }
            else if (status == "Long Default")
            {
                SetStopLoss(CalculationMode.Price, stopLossPrice);
            }
            else if (status == "Breakeven")
            {
                SetStopLoss(CalculationMode.Price, Position.AveragePrice);
            }
            else if (status == "Breakeven2 Long")
            {
                SetStopLoss(CalculationMode.Ticks, Trail2LevelLong );
            }
            else if (status == "Breakeven2 Short")
            {
                SetStopLoss(CalculationMode.Ticks, Trail2LevelShort);
            }
        }

        private void setUserPoisitonSizes()
        {
            LotSize1 = UserLotSize1;
            LotSize2 = UserLotSize2;
            LotSize3 = UserLotSize3;
        }


        private void setSmallPositionSizes()
        {
            LotSize1 = 1;
            LotSize2 = 1;
            LotSize3 = 1;
        }

        private bool applyFilters()
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

        private string calculateSignal()
        {
            string signalType = "No Signal";
            double recentSpanA = ApolloIchimoku(BarsArray[0], 9, 26, 52, 26).SpanALine[26];
            double recentSpanB = ApolloIchimoku(BarsArray[0], 9, 26, 52, 26).SpanBLine[26];
            double conversionLine = ApolloIchimoku(BarsArray[0], 9, 26, 52, 26).ConversionLine[0];
            double baseline = ApolloIchimoku(BarsArray[0], 9, 26, 52, 26).BaseLine[0];
            string cloud = calculateCloud(recentSpanA, recentSpanB);

            if (conversionLine > baseline && cloud == "Green" && IsAroonUptrend())
            {
                signalType = "Strong Long";
            }
            else if (conversionLine > baseline && cloud == "Red" && IsAroonUptrend())
            {
                signalType = "Weak Long";
            }
            else if (conversionLine > baseline && cloud == "Green" && IsAroonDowntrend())
            {
                signalType = "Weak Long";
            }

            else if (conversionLine < baseline && cloud == "Red" && IsAroonDowntrend())
            {
                signalType = "Strong Short";
            }

            else if (conversionLine < baseline && cloud == "Green" && IsAroonDowntrend())
            {
                signalType = "Weak Short";
            }

            else if (conversionLine < baseline && cloud == "Red" && IsAroonUptrend())
            {
                signalType = "Weak Short";
            }



            return signalType;

        }

        private string calculateCloud(double recentSpanA, double recentSpanB)
        {

            if (recentSpanA > recentSpanB)
            {
                return "Green";
            }
            else
            {
                return "Red";
            }
        }

        #region Entry Positions

        private bool LongConditions()
        {
            return noPositions() && previousCandleGreen() && IsAroonUptrend() && stochRsiEntry(RsiEntryLong, "Long");
        }


        private bool ShortConditions()
        {
            return noPositions() && previousCandleRed() && IsAroonDowntrend() && stochRsiEntry(RsiEntryShort, "Short");
        }


        #endregion

        private bool previousCandleRed()
        {
            return HeikenAshi8(BarsArray[0]).HAOpen[0] > HeikenAshi8(BarsArray[0]).HAClose[0] && HeikenAshi8(BarsArray[0]).HAOpen[1] < HeikenAshi8(BarsArray[0]).HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8(BarsArray[0]).HAOpen[0] < HeikenAshi8(BarsArray[0]).HAClose[0] && HeikenAshi8(BarsArray[0]).HAOpen[1] > HeikenAshi8(BarsArray[0]).HAClose[1];
        }

        private bool IsAroonUptrend()
        {
            return Aroon(BarsArray[2],AroonPeriod).Up[0] > 70;
        }

        private bool IsAroonDowntrend()
        {
            return Aroon(BarsArray[2], AroonPeriod).Down[0] > 70;
        }


        private bool noPositions()
        {
            return Position.MarketPosition == MarketPosition.Flat;
        }

        private bool stochRsiEntry(int entryValue, string positionType)
        {
            _stochFast = StochRSIMod2NT8(BarsArray[0],StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
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

        private bool stochRsiExtra(int entryValue, string positionType)
        {
           double _stochFastPrevious = StochRSIMod2NT8(BarsArray[0],StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
           double _stochFastLast = StochRSIMod2NT8(BarsArray[0],StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[0];
            if (positionType=="Short" && _stochFastPrevious > entryValue && _stochFastLast < entryValue )
            {
                return true;
            }
            else if(positionType== "Long" && _stochFastPrevious < entryValue && _stochFastLast > entryValue)
            {
                return true;
            }else
            {
                return false;
            }
         }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
        OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (OrderFilled(order) && IsLongOrder1(order))
            {
                _longEntryPrice1 = averageFillPrice;
                stopLossPrice = calculateStopLong();
                SetProfitTarget("Long1", CalculationMode.Price, _longEntryPrice1 + _atrValue * LongAtrRatio);
                status = "Long Default";
            }
            else if (OrderFilled(order) && IsLongOrder2(order))
            {
                  SetProfitTarget("Long2", CalculationMode.Price, _longEntryPrice1 + _atrValue * LongAtrRatio);
         //       SetProfitTarget("Long2", CalculationMode.Price, _longEntryPrice1 + ProfitTargetLong1* TickSize);
         //       status = "Long Extra";
            }
            else if (OrderFilled(order) && IsLongOrder3(order))
            {
                SetProfitTarget("Long3", CalculationMode.Ticks, ProfitTargetLong3);
            }

            else if (OrderFilled(order) && IsShortOrder1(order))
            {
                _shortEntryPrice1 = averageFillPrice;
                stopLossPrice = calculateStopShort();
        //        SetProfitTarget("Short1", CalculationMode.Price, _shortEntryPrice1 - _atrValue * ShortAtrRatio);
                    SetProfitTarget("Short1", CalculationMode.Ticks, ProfitTargetShort1);
                status = "Short Default";
            }
            else if (OrderFilled(order) && IsShortOrder2(order))
            {
                //SetProfitTarget("Short2", CalculationMode.Price, _shortEntryPrice1 - _atrValue * ShortAtrRatio);
                      SetProfitTarget("Short2", CalculationMode.Ticks, ProfitTargetShort1);
                //       SetProfitTarget("Short2", CalculationMode.Price, _shortEntryPrice1 - ProfitTargetShort1 * TickSize);
                //    status = "Short Extra";
            }
            else if (OrderFilled(order) && IsShortOrder3(order))
            {
                SetProfitTarget("Short3", CalculationMode.Ticks, ProfitTargetShort3);
            }

            //   MonitorStopProfit(order, limitPrice, stopPrice);
            //        MoveStopToBreakeven(order);

        }


        #region tradeTime
        private void CalculateTradeTime()
        {

            if ((ToTime(Time[0]) >= 153000 && ToTime(Time[0]) < 213000))
            {
                _canTrade = true;
            }
            else
            {
                _canTrade = false;
            }
        }

        #endregion

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

        #region Stop Calculation
        private double calculateStopLong()
        {
            List<double> lows = new List<double> { };
            int i = 0;
            while (i < BarsToCheck)
            {
                lows.Add(Lows[0][i]);

                i++; // increment
            }
            lows.Sort();

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

        private double calculateStopShort()
        {
            List<double> highs = new List<double> { };
            int i = 0;
            while (i < BarsToCheck)
            {
                highs.Add(Highs[0][i]);

                i++; // increment
            }
            highs.Sort();
            highs.Reverse();
            double highestHigh = highs[0];

            double dynamicStopLoss = highestHigh + ShortStopMargin * TickSize;
            double maxStopLoss = _shortEntryPrice1 + MaxLossMargin * TickSize;

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

        private void ShowInfo()
        {
            double recentSpanA = ApolloIchimoku(BarsArray[0],9, 26, 52, 26).SpanALine[26];
            double recentSpanB = ApolloIchimoku(BarsArray[0],9, 26, 52, 26).SpanBLine[26];
            Print(Time[0]);
            Print("Price:");
            Print(Close[0]);
            Print("ICHIMOKU");
            Print("**************");
            Print("ConversionLine");
            Print(ApolloIchimoku(BarsArray[0],9, 26, 52, 26).ConversionLine[0]);
            Print("BaseLine");
            Print(ApolloIchimoku(BarsArray[0],9, 26, 52, 26).BaseLine[0]);
            Print("SpanALine");
            Print(recentSpanA);
            Print("SpanBLine");
            Print(recentSpanB);

            if (recentSpanA > recentSpanB)
            {
                Print("Cloud is Green");
            }
            else if (recentSpanB > recentSpanA)
            {
                Print("Cloud is Red");
            }
            Print("~~~~~~~~~~~~~~~~~~~~");
        }

        private void ShowEntryDetails(string signal)
        {
            Print("~~~~~~~~~~~~~~~~~~~~~~~~");
            Print("ENTRY DETAILS:");
            Print(signal);
            Print(Time[0]);
            Print("IB High:");
            Print(Levels(BarsArray[0]).IBHighs[0]);
         //   Print("IB Low:");
       //     Print(Levels(BarsArray[0]).IBLows[0]);
            Print("~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        private void MonitorStopProfit(Order order, double limitPrice, double stopPrice)
        {
            if (order.OrderState == OrderState.Submitted)
            {
                // Add the "Stop loss" orders to the Stop Loss collection
                if (order.Name == "Stop loss")
                    stopLossOrders.Add(order);

                // Add the "Profit target" orders to the Profit Target collection
                else if (order.Name == "Profit target")
                    profitTargetOrders.Add(order);
            }

            // Process stop loss orders
            if (stopLossOrders.Contains(order))
            {
                // Check order for terminal state
                if (order.OrderState == OrderState.Cancelled || order.OrderState == OrderState.Filled || order.OrderState == OrderState.Rejected)
                {
                    // Print out information about the order
                    Print(order);

                    // Remove from collection
                    stopLossOrders.Remove(order);
                }

                else
                {
                    // Print out the current stop loss price
                    Print("The order name " + order.Name + " stop price is currently " + stopPrice);
                }
            }

            // Process profit target orders
            if (profitTargetOrders.Contains(order))
            {
                // Check order for terminal state
                if (order.OrderState == OrderState.Cancelled || order.OrderState == OrderState.Filled || order.OrderState == OrderState.Rejected)
                {
                    // Print out information about the order
                    Print(order);

                    // Remove from collection
                    profitTargetOrders.Remove(order);
                }
                else
                {
                    // Print out the current stop loss price
                    Print("The order name " + order.Name + " limit price is currently " + limitPrice);
                }
            }
        }



        private void addIndicators()
        {
            _ha = HeikenAshi8(BarsArray[0]);
            _aroon = Aroon(BarsArray[1],AroonPeriod);
            _stoch = StochRSIMod2NT8(BarsArray[0],StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack);
            _iczimoku = ApolloIchimoku(BarsArray[0],9, 26, 52, 26);
            AddChartIndicator(_ha);
            AddChartIndicator(_stoch);
            AddChartIndicator(_aroon);
            AddChartIndicator(_iczimoku);
            _atr = ATR(BarsArray[2],AtrPeriod);
            AddChartIndicator(_atr);
            _Vix = WVF(BarsArray[3]);
        }
    }
}

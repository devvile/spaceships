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
    public class Sputnik_V_Multi : Strategy
    {
        #region Declarations
        private int _lotSize1;
        private int _lotSize2;
        private int _lotSize3;
        private Indicator _ha;
        private Indicator _haD;
        private Indicator _aroon;
        private Indicator _stoch;
        private Indicator _pSar;
        private Indicator _emaFast;
        private Indicator _emaSlow;
        private Indicator _hmaFastDay;
        private Indicator _hmaSlowDay;

        Random rnd = new Random();
        private int _aroonPeriod;
        private bool _useAroon;
        private bool _showHA = true;
        private bool _useRsi = false;
        private bool _makeTrades = false;
        private bool _useLongs = true;
        private bool _useShorts = true;
        private bool _canExtra = true;
        private bool _useHMA = true;
        private double _stochFast;

        private int _emaFastPeriod;
        private int _emaSlowPeriod;

        private double _longEntryPrice1;
        private double _stopLossBaseLong;
        private int _profitTargetLong1;
        private int _profitTargetLong2;
        private int _profitTargetLong3;
        private Order _longOneOrder;
        private Order _longTwoOrder;
        private Order _longThreeOrder;
        private Order _longFourOrder;
        private Order _longFiveOrder;
        private Order _longSixOrder;
        private int _baseStopMarginLong;
        private double _extraTargetLong1;
        private double _extraTargetLong2;
        private double _extraTargetLong3;

        private double _shortEntryPrice1;
        private double _stopLossBaseShort;
        private int _profitTargetShort1;
        private int _profitTargetShort2;
        private int _profitTargetShort3;
        private Order _shortOneOrder;
        private Order _shortTwoOrder;
        private Order _shortThreeOrder;
        private Order _shortFourOrder;
        private Order _shortFiveOrder;
        private Order _shortSixOrder;
        private int _baseStopMarginShort;
        private double _extraTargetShort1;
        private double _extraTargetShort2;
        private double _extraTargetShort3;

        private int _extraSize;
        #endregion

        #region Parameters

        private int _rsiEntryShort;

        private int _rsiEntryLong;

        private int _stochRsiPeriod;
        private int _fastMAPeriod;
        private int _slowMAPeriod;
        private int _lookBack;
        private string status = "Flat";

     //   private int _minuteIndex =0 ;
        #endregion


        #region Config

        [Display(Name = "Aroon Period", GroupName = "Config", Order = 0)]
        public int AroonPeriod
        {
            get { return _aroonPeriod; }
            set { _aroonPeriod = value; }
        }

        [Display(Name = "Show HA", GroupName = "Config", Order = 0)]
        public bool ShowHA
        {
            get { return _showHA; }
            set { _showHA = value; }
        }


        [Display(Name = "Use Aroon", GroupName = "Config", Order = 0)]
        public bool UseAroon
        {
            get { return _useAroon; }
            set { _useAroon = value; }
        }

        [Display(Name = "Use RSI", GroupName = "Config", Order = 0)]
        public bool UseRsi
        {
            get { return _useRsi; }
            set { _useRsi = value; }
        }

        [Display(Name = "Can Extra", GroupName = "Config", Order = 0)]
        public bool CanExtra
        {
            get { return _canExtra; }
            set { _canExtra = value; }
        }

        [Display(Name = "Trade", GroupName = "Config", Order = 0)]
        public bool makeTrades
        {
            get { return _makeTrades; }
            set { _makeTrades = value; }
        }

        [Display(Name = "Use HMA", GroupName = "Config", Order = 0)]
        public bool UseHMA
        {
            get { return _useHMA; }
            set { _useHMA = value; }
        }

        #endregion

        #region Position Management

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

        [Display(Name = "Extra Size", GroupName = "Position Management", Order = 0)]
        public int ExtraSize
        {
            get { return _extraSize; }
            set { _extraSize = value; }
        }
        #endregion

        #region Long

        [Display(Name = "Use Longs", GroupName = "Long", Order = 0)]
        public bool UseLongs
        {
            get { return _useLongs; }
            set { _useLongs = value; }
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

        [Display(Name = "Base Stop Margin", GroupName = "Long", Order = 0)]
        public int BaseStopMarginLong
        {
            get { return _baseStopMarginLong; }
            set { _baseStopMarginLong = value; }
        }

        #endregion

        #region Short

        [Display(Name = "Use Shorts", GroupName = "Short", Order = 0)]
        public bool UseShorts
        {
            get { return _useShorts; }
            set { _useShorts = value; }
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

        [Display(Name = "Base Stop Margin", GroupName = "Short", Order = 0)]
        public int BaseStopMarginShort
        {
            get { return _baseStopMarginShort; }
            set { _baseStopMarginShort = value; }
        }


        #endregion

        #region Extra

        [Display(Name = "EMA Fast Period", GroupName = "Extra", Order = 0)]
        public int EmaFastPeriod
        {
            get { return _emaFastPeriod; }
            set { _emaFastPeriod = value; }
        }

        [Display(Name = "EMA Skiw Period", GroupName = "Extra", Order = 0)]
        public int EmaSlowPeriod
        {
            get { return _emaSlowPeriod; }
            set { _emaSlowPeriod = value; }
        }

        [Display(Name = "Extra Target Long 1", GroupName = "Extra", Order = 0)]
        public double ExtraTargetLong1
        {
            get { return _extraTargetLong1; }
            set { _extraTargetLong1 = value; }
        }

        [Display(Name = "Extra Target Long 2", GroupName = "Extra", Order = 0)]
        public double ExtraTargetLong2
        {
            get { return _extraTargetLong2; }
            set { _extraTargetLong2 = value; }
        }

        [Display(Name = "Extra Target Long 3", GroupName = "Extra", Order = 0)]
        public double ExtraTargetLong3
        {
            get { return _extraTargetLong3; }
            set { _extraTargetLong3 = value; }
        }


        [Display(Name = "Extra Target Short 1", GroupName = "Extra", Order = 0)]
        public double ExtraTargetShort1
        {
            get { return _extraTargetShort1; }
            set { _extraTargetShort1 = value; }
        }

        [Display(Name = "Extra Target Short 2", GroupName = "Extra", Order = 0)]
        public double ExtraTargetShort2
        {
            get { return _extraTargetShort2; }
            set { _extraTargetShort2 = value; }
        }


        [Display(Name = "Extra Target Short 3", GroupName = "Extra", Order = 0)]
        public double ExtraTargetShort3
        {
            get { return _extraTargetShort3; }
            set { _extraTargetShort3 = value; }
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
                Description = @"Sputnik Refacatored";
                Name = "Sputnik V reborn multiframe";
                Calculate = Calculate.OnBarClose;
                _lotSize1 = 1;
                _lotSize2 = 1;
                _lotSize3 = 1;
                _profitTargetLong1 = 12;
                _profitTargetLong2 = 12;
                _profitTargetLong3 = 12;
                _baseStopMarginLong = 18;
                _baseStopMarginShort = 18;
                _profitTargetShort1 = 12;
                _profitTargetShort2 = 12;
                _profitTargetShort3 = 12;
                _stochRsiPeriod = 9;
                _fastMAPeriod = 3;
                _slowMAPeriod = 3;
                _lookBack = 14;

                _aroonPeriod = 25;
                _rsiEntryLong = 20;
                _rsiEntryShort = 80;

                _extraSize = 1;
                _extraTargetLong1=1;
                _extraTargetLong2=1;
                _extraTargetLong3=1;
                _extraTargetShort1=1;
                _extraTargetShort2=1;
                _extraTargetShort3=1;
    }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;
                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                AddDataSeries(BarsPeriodType.Day, 1);
       //         AddDataSeries(BarsPeriodType.Tick, 1);
             //   AddHeikenAshi("MES 09-23", BarsPeriodType.Minute, 1, MarketDataType.Last);
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
            if (CurrentBars[0] <= BarsRequiredToTrade || CurrentBars[1] <= BarsRequiredToTrade  ) return;

            if(BarsInProgress == 0){ 

        
                if (UseLongs)
                {
                    if (LongConditions())
                    {
                       if (makeTrades)
                       {
                          _longOneOrder = EnterLong(LotSize1, "Basic Long Entry1");
                          _longTwoOrder = EnterLong(LotSize2, "Basic Long Entry2");
                          _longThreeOrder = EnterLong(LotSize3, "Basic Long Entry3");
                       }
                       else
                       {
                          Mark("Long");
                       }
                    }
                    if (LongExtraCondition1())
                    {
                        if (makeTrades)
                        {
                            _longFourOrder = EnterLong(ExtraSize, "Extra Long Entry1");
                            _longFiveOrder = EnterLong(ExtraSize, "Extra Long Entry2");
                            _longSixOrder = EnterLong(ExtraSize, "Extra Long Entry3");
                            status = "Extra Long";
                        }
                        else 
                        {
                            Mark("Extra Long");
                        }
                    }
                }

                if (UseShorts)
                {
                    if (ShortConditions())
                    { 
                        if (makeTrades)
                        {
                            _shortOneOrder = EnterShort(LotSize1, "Basic Short Entry1");
                            _shortTwoOrder = EnterShort(LotSize2, "Basic Short Entry2");
                            _shortThreeOrder = EnterShort(LotSize3, "Basic Short Entry3");
                        }
                        else
                        {
                            Mark("Short");
                        }
                    }
                    if (ShortExtraCondition1())
                    {
                        if (makeTrades)
                        {
                            _shortFourOrder = EnterShort(ExtraSize, "Extra Short Entry1");
                            _shortFiveOrder = EnterShort(ExtraSize, "Extra Short Entry2");
                            _shortSixOrder = EnterShort(ExtraSize, "Extra Short Entry3");
                            status = "Extra Short";
                        }
                        else 
                        {
                            Mark("Extra Short");
                        }
                    }
                }

            }

        }
        #region Entry Positions

        private bool LongConditions()
        {
            return noPositions() && previousCandleGreen() && IsAroonUptrend() && stochRsiEntry(RsiEntryLong ,"Long") && isHMAUptrend();
        }

        private bool LongExtraCondition1()
        {
            return Position.MarketPosition == MarketPosition.Long && (CrossAbove(_emaFast, _emaSlow, 1) && _pSar[0] < Close[0]) && CanExtra; // && _canExtra && _canTrade; ; // && EMA 15  i 5 oraz SAR ;
        }

        private bool ShortConditions()
        {
            return noPositions() && previousCandleRed() && IsAroonDowntrend() && stochRsiEntry(RsiEntryShort, "Short") && isHMADowntrend();
        }

        private bool ShortExtraCondition1()
        {
            return Position.MarketPosition == MarketPosition.Short && (CrossBelow(_emaFast, _emaSlow,1) && _pSar[0] > Close[0]) && CanExtra;// && _canExtra && _canTrade; ; //&& EMA 15  i 5 oraz SAR ;

        }

        #endregion  

        private bool previousCandleRed()
        {
            return HeikenAshi8().HAOpen[0] > HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] < HeikenAshi8().HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8().HAOpen[0] < HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] > HeikenAshi8().HAClose[1];
        }

        private void Mark(string positionType)
        {
            int _nr = rnd.Next();
            string rando = Convert.ToString(_nr);
            string name = "tag " + rando;
            if(positionType == "Short")
            {
                Draw.ArrowDown(this, name, true, 0, High[0] + TickSize, Brushes.Red);
            }else if(positionType == "Extra Short")
            {
                Draw.ArrowDown(this, name, true, 0, High[0] + TickSize, Brushes.Yellow);
            }
            else if (positionType == "Long")
            {
                Draw.ArrowUp(this, name, true, 0, Low[0] - TickSize, Brushes.Blue);
            }
            else if (positionType == "Extra Long")
            {
                Draw.ArrowUp(this, name, true, 0, Low[0] - TickSize, Brushes.Yellow);
            }

        }


        private bool IsAroonUptrend()
        {
            if (UseAroon)
            {
                return Aroon(AroonPeriod).Up[0] > Aroon(AroonPeriod).Down[0];
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
                return Aroon(AroonPeriod).Up[0] < Aroon(AroonPeriod).Down[0];
            }
            else
            {
                return true;
            }

        } 

        private bool noPositions()
        {
            return Position.MarketPosition == MarketPosition.Flat;
        }

        private bool isHMAUptrend()
        {
            if (UseHMA)
            {
                return _hmaFastDay[0] > _hmaSlowDay[0];
            }
            else
            {
                return true;
            }
        }

        private bool isHMADowntrend()
        {

            if (UseHMA)
            {
                return _hmaFastDay[0]< _hmaSlowDay[0];
            }
            else
            {
                return true;
            }
        }

        private bool stochRsiEntry(int entryValue, string positionType )
        {
            if (UseRsi)
            {
                _stochFast = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
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

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (OrderFilled(order) && IsLongOrder1(order))
            {
                _longEntryPrice1 = averageFillPrice;
                _stopLossBaseLong = calculateStopLong();
                SetStopLoss("Basic Long Entry1", CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Basic Long Entry1", CalculationMode.Ticks, ProfitTargetLong1);
                status = "Long 1 Default";
            }
            else if (OrderFilled(order) && IsLongOrder2(order))
            {
                SetStopLoss("Basic Long Entry2", CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Basic Long Entry2", CalculationMode.Ticks, ProfitTargetLong2);
            }
            else if (OrderFilled(order) && IsLongOrder3(order))
            {
                SetStopLoss("Basic Long Entry3", CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Basic Long Entry3", CalculationMode.Ticks, ProfitTargetLong3);
            }
            else if (OrderFilled(order) && IsLongOrder4(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopLong());
                SetProfitTarget("Extra Long Entry1", CalculationMode.Ticks, ExtraTargetLong1);
            }

            else if (OrderFilled(order) && IsLongOrder5(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopLong());
                SetProfitTarget("Extra Long Entry2", CalculationMode.Ticks, ExtraTargetLong2);
            }

            else if (OrderFilled(order) && IsLongOrder6(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopLong());
                SetProfitTarget("Extra Long Entry3", CalculationMode.Ticks, ExtraTargetLong3);
            }


            else if (OrderFilled(order) && IsShortOrder1(order))
            {
                _shortEntryPrice1 = averageFillPrice;
                _stopLossBaseShort = calculateStopShort();
                SetStopLoss("Basic Short Entry1", CalculationMode.Price, _stopLossBaseShort, false);
                SetProfitTarget("Basic Short Entry1", CalculationMode.Ticks, ProfitTargetShort1);
                status = "Short 1 Default";
            }
            else if (OrderFilled(order) && IsShortOrder2(order))
            {
                SetStopLoss("Basic Short Entry2", CalculationMode.Price, _stopLossBaseShort, false);
                SetProfitTarget("Basic Short Entry2", CalculationMode.Ticks, ProfitTargetShort2);
            }
            else if (OrderFilled(order) && IsShortOrder3(order))
            {
                SetStopLoss("Basic Short Entry3", CalculationMode.Price, _stopLossBaseShort, false);
                SetProfitTarget("Basic Short Entry3", CalculationMode.Ticks, ProfitTargetShort3);
            }
            else if (OrderFilled(order) && IsShortOrder4(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopShort());
                SetProfitTarget("Extra Short Entry1", CalculationMode.Ticks, ExtraTargetShort1);
            }
            else if (OrderFilled(order) && IsShortOrder5(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopShort());
                SetProfitTarget("Extra Short Entry2", CalculationMode.Ticks, ExtraTargetShort2);
            }
            else if (OrderFilled(order) && IsShortOrder6(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopShort());
                SetProfitTarget("Extra Short Entry3", CalculationMode.Ticks, ExtraTargetShort3);
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

        private bool IsLongOrder4(Order order)
        {
            return order == _longFourOrder;
        }

        private bool IsLongOrder5(Order order)
        {
            return order == _longFiveOrder;
        }

        private bool IsLongOrder6(Order order)
        {
            return order == _longSixOrder;
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

        private bool IsShortOrder4(Order order)
        {
            return order == _shortFourOrder;
        }

        private bool IsShortOrder5(Order order)
        {
            return order == _shortFiveOrder;
        }

        private bool IsShortOrder6(Order order)
        {
            return order == _shortSixOrder;
        }

        private bool OrderFilled(Order order)
        {
            return order.OrderState == OrderState.Filled;
        }

        #endregion

        #region Stop Calculation
        private double calculateStopLong()
        {
            List<double> lows = new List<double> {Low[0], Low[1], Low[2], Low[3], Low[4] };
            lows.Sort();
            double lowestLow = lows[0];
            double baseStopLoss = lowestLow - BaseStopMarginLong * TickSize;

            return baseStopLoss;
        }

        private double calculateStopShort()
        {
            List<double> highs = new List<double> {High[0], High[1], High[2], High[3], High[4] };
            highs.Sort();
            highs.Reverse();
            double highestHigh = highs[0];
            double baseStopLoss = highestHigh + BaseStopMarginShort * TickSize;

            return highestHigh;
        }
        #endregion

        private void addIndicators()
        {

            _ha = HeikenAshi8(BarsArray[0]);
            _haD = HeikenAshi8();
            _aroon = Aroon(BarsArray[0],AroonPeriod);
            _stoch = StochRSIMod2NT8( StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack);

            _pSar = ParabolicSAR(BarsArray[0], 0.02, 0.2, 0.02);
            _emaFast = EMA(BarsArray[0], EmaFastPeriod);
            _emaSlow = EMA(BarsArray[0],EmaSlowPeriod);
            _hmaFastDay = HMA(BarsArray[1], 5);
            _hmaSlowDay = HMA(BarsArray[1], 15);

            if (UseHMA)
            {
                AddChartIndicator(_hmaFastDay);
                AddChartIndicator(_hmaSlowDay);
            }


            if (ShowHA)
            {
                AddChartIndicator(_ha);
                _haD = HeikenAshi8();
            }
            AddChartIndicator(_stoch);
            AddChartIndicator(_aroon);

            if (CanExtra)
            {
                AddChartIndicator(_pSar);
                AddChartIndicator(_emaFast);
                AddChartIndicator(_emaSlow);
            }


            //       AddChartIndicator(IchimokuCloud());
        }
    }
}

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

namespace NinjaTrader.NinjaScript.Strategies
{
    public class SputnikIVStoch : Strategy
    {
        #region declarations
        private Indicator _sma;
        private Indicator _emaMin;
        private Indicator _emaMax;
        private Indicator _pSar;

        private double _stochFast;

        private Order _longOneOrder;
        private Order _longTwoOrder;
        private Order _longThreeOrder;
        private Order _longFourOrder;
        private Order _longFiveOrder;
        private Order _longSixOrder;
        private double _shortStopSwitch1Ticks;
        private double _stopLossBaseShort;
        private double _shortEntryPrice1;

        private Order _shortOneOrder;
        private Order _shortTwoOrder;
        private Order _shortThreeOrder;
        private Order _shortFourOrder;
        private Order _shortFiveOrder;
        private Order _shortSixOrder;

        private Order _longTargetOrder1;
        private double _longEntryPrice1;
        private double _longStopSwitch1Ticks;
        private double _stopLossBaseLong;

        #endregion

        #region My Parameters

        private double _profitTarget = 16;
        private double _profitTargetLong2 = 20;
        private double _profitTargetLong3 = 30;
        private double _extraTargetLong1 = 10;
        private double _extraTargetLong2 = 20;
        private double _extraTargetLong3 = 30;
        private int _entryFastStochValueLong = 10;
        private int _baseStopMarginLong = 18;


        private double _profitTargetShort = 16;
        private double _profitTargetShort2 = 20;
        private double _profitTargetShort3 = 30;
        private double _extraTargetShort1 = 10;
        private double _extraTargetShort2 = 20;
        private double _extraTargetShort3 = 30;
        private int _entryFastStochValueShort = 90;
        private int _baseStopMarginShort = 18;

        private int _stochRsiPeriod = 9;
        private int _fastMAPeriod = 3;
        private int _slowMAPeriod = 3;
        private int _lookBack = 14;



        private int _emaMinPeriod = 5;
        private int _emaMaxPeriod = 15;

        private bool _canExtra = true;
        private bool _useEma = true;
        private bool _useLongs = true;
        private bool _useShorts = true;
        private bool _reset = true;
        private int _lotSize1 = 2;
        private int _lotSize2 = 4;
        private int _lotSize3 = 1;
        private int _extraSize = 3;
        private int _barNr = 0;  //sprawdzic <-----
        Random rnd = new Random();

        private string status = "Flat";


        #endregion

        #region GENERAL

        [Display(Name = "Bar NR", GroupName = "General", Order = 0)]
        public int BarNr
        {
            get { return _barNr; }
            set { _barNr = value; }
        }

        [Display(Name = "Reset Strategy", GroupName = "General", Order = 0)]
        public bool Reset
        {
            get { return _reset; }
            set { _reset = value; }
        }
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



        [Display(Name = "Profit Target Long 1", GroupName = "LONGS", Order = 0)]
        public double ProfitTarget
        {
            get { return _profitTarget; }
            set { _profitTarget = value; }
        }

        [Display(Name = "Profit Target Long 2", GroupName = "LONGS", Order = 0)]
        public double ProfitTargetLong2
        {
            get { return _profitTargetLong2; }
            set { _profitTargetLong2 = value; }
        }

        [Display(Name = "Profit Target Long 3", GroupName = "LONGS", Order = 0)]
        public double ProfitTargetLong3
        {
            get { return _profitTargetLong3; }
            set { _profitTargetLong3 = value; }
        }

        [Display(Name = "Extra Target Long 1", GroupName = "LONGS", Order = 0)]
        public double ExtraTargetLong1
        {
            get { return _extraTargetLong1; }
            set { _extraTargetLong1 = value; }
        }

        [Display(Name = "Extra Target Long 2", GroupName = "LONGS", Order = 0)]
        public double ExtraTargetLong2
        {
            get { return _extraTargetLong2; }
            set { _extraTargetLong2 = value; }
        }

        [Display(Name = "Extra Target Long 3", GroupName = "LONGS", Order = 0)]
        public double ExtraTargetLong3
        {
            get { return _extraTargetLong3; }
            set { _extraTargetLong3 = value; }
        }

        [Display(Name = "Stop Switch Long Level 1", GroupName = "LONGS", Order = 0)]
        public double StopSwitchLong1
        {
            get { return _longStopSwitch1Ticks; }
            set { _longStopSwitch1Ticks = value; }
        }

        [Display(Name = "Base Stop Margin Long", GroupName = "LONGS", Order = 0)]
        public int BaseStopMarginLong
        {
            get { return _baseStopMarginLong; }
            set { _baseStopMarginLong = value; }
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

        [Display(Name = "Stop Switch Short Level 1", GroupName = "SHORTS", Order = 0)]
        public double StopSwitchShort1
        {
            get { return _shortStopSwitch1Ticks; }
            set { _shortStopSwitch1Ticks = value; }
        }


        [Display(Name = "Profit Target Short", GroupName = "SHORTS", Order = 0)]
        public double ProfitTargetShort
        {
            get { return _profitTargetShort; }
            set { _profitTargetShort = value; }
        }

        [Display(Name = "Profit Target Short 2", GroupName = "SHORTS", Order = 0)]
        public double ProfitTargetShort2
        {
            get { return _profitTargetShort2; }
            set { _profitTargetShort2 = value; }
        }

        [Display(Name = "Profit Target Short 3", GroupName = "SHORTS", Order = 0)]
        public double ProfitTargetShort3
        {
            get { return _profitTargetShort3; }
            set { _profitTargetShort3 = value; }
        }

        [Display(Name = "Extra Target Short 1", GroupName = "SHORTS", Order = 0)]
        public double ExtraTargetShort1
        {
            get { return _extraTargetShort1; }
            set { _extraTargetShort1 = value; }
        }

        [Display(Name = "Extra Target Short 2", GroupName = "SHORTS", Order = 0)]
        public double ExtraTargetShort2
        {
            get { return _extraTargetShort2; }
            set { _extraTargetShort2 = value; }
        }


        [Display(Name = "Extra Target Short 3", GroupName = "SHORTS", Order = 0)]
        public double ExtraTargetShort3
        {
            get { return _extraTargetShort3; }
            set { _extraTargetShort3 = value; }
        }



        [Display(Name = "Base Stop Margin Short", GroupName = "SHORTS", Order = 0)]
        public int BaseStopMarginShort
        {
            get { return _baseStopMarginShort; }
            set { _baseStopMarginShort = value; }
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


        [Display(Name = "Position Size 3", GroupName = "Position Management", Order = 0)]
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

        #region Filters


        #endregion

        #region EMA

        [Display(Name = "Use EMA", GroupName = "EMA", Order = 0)]
        public bool UseEma
        {
            get { return _useEma; }
            set { _useEma = value; }
        }


        [Display(Name = "EMA Min PERIOD", GroupName = "EMA", Order = 0)]
        public int EmaMinPeriod
        {
            get { return _emaMinPeriod; }
            set { _emaMinPeriod = value; }
        }


        [Display(Name = "EMA Max PERIOD", GroupName = "EMA", Order = 0)]
        public int EmaMaxPeriod
        {
            get { return _emaMaxPeriod; }
            set { _emaMaxPeriod = value; }
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
                Description = @"Sputnik IV using stochastic";
                Name = "Sputnik IV Stoch";
                Calculate = Calculate.OnBarClose;
                BarsRequiredToTrade = 20;

            }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;

                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                AddHeikenAshi("MES 09-23", BarsPeriodType.Minute, 1, MarketDataType.Last);
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

            if (BarsInProgress == 1)
            {
                Print("HAIKEN ASHI!");
                Print(Time[0]);
                Print("CLOSE" + Close[0]);
                Print("HIGH" + High[0]);
                Print("LOW" + Low[0]);
                Print("OPEN" + Open[0]);
                Print("~~~~~~~~~~~~~~");
            }

            if (Position.MarketPosition == MarketPosition.Flat)
                {
                    _canExtra = true;
                }
                else if (Position.MarketPosition == MarketPosition.Short && Reset)
                {
                    ExitShort();
                    Reset = false;
                }
                else if (Position.MarketPosition == MarketPosition.Long && Reset)
                {
                    Print("Exiting Long...");
                    ExitLong();
                    Reset = false;
                }
    

            if (UseLongs)
            {
                if (LongCondition1())
                {
                    _longOneOrder = EnterLong(LotSize1,  "Long Entry1");
                    _longTwoOrder = EnterLong(LotSize2,  "Long Entry2");
                    _longThreeOrder = EnterLong(LotSize3, "Long Entry3");
                }
                if (LongExtraCondition1())
                {
                    Print("EXTRA LONGGGGGG~!!!!!!");
                    _longFourOrder = EnterLong(ExtraSize, "Long Entry4");
                    _longFiveOrder = EnterLong(ExtraSize, "Long Entry5");
                    _longSixOrder = EnterLong(ExtraSize, "Long Entry6");
                    _canExtra = false;
                    status = "Extra Long";
                }


                if (firstTrailConditionLong()) 
                {
                    Print("Moving stop to Breakeven");
                    SetStopLoss(CalculationMode.Price, _longEntryPrice1 + (1 * TickSize));
                    status = "Long Stop Breakeven";
                }
            }
            if (UseShorts)
            {
                if (ShortCondition1())
                {

                    _shortOneOrder = EnterShort(LotSize1, "Short Entry1");
                    _shortTwoOrder = EnterShort(LotSize2, "Short Entry2");
                    _shortThreeOrder = EnterShort(LotSize3, "Short Entry3");

                }

                if (ShortExtraCondition1())
                {
                    Print("EXTRA SHORTTTTT ~!!!!!!");
                    _shortFourOrder = EnterShort(ExtraSize, "Short Entry4");
                    _shortFiveOrder = EnterShort(ExtraSize, "Short Entry5");
                    _shortSixOrder = EnterShort(ExtraSize, "Short Entry6");
                    _canExtra = false;
                    status = "Extra Short";
                }

                if (firstTrailConditionShort())
                { 
                    Print("Moving stop to Breakeven");
                    Print("Print TUTAJ KRUWA TUTAJ");
                    Print(_shortEntryPrice1);
                    Print("NEW LEVEL SL");
                    Print(_shortEntryPrice1 - (3 * TickSize));
                     SetStopLoss(CalculationMode.Price, _shortEntryPrice1 - (1 * TickSize));
            
                }


            }

        }

        private bool firstTrailConditionLong()
        {
            if (status == "Long 2 Default" || status == "Long 1 Default" && status != "Extra Long")
            {
                return Position.MarketPosition == MarketPosition.Long && Close[BarNr] >= _longEntryPrice1 + (StopSwitchLong1 * TickSize) && status != "Long Stop Breakeven";
            }
            else
            {
                return false;
            }

        }


        private bool LongExtraCondition1()
        {
            return Position.MarketPosition == MarketPosition.Long && (CrossAbove(_emaMin, _emaMax, 1) && _pSar[0] < Closes[1][0]) && _canExtra; // && EMA 15  i 5 oraz SAR ;
        }

        private bool ShortExtraCondition1()
        {
            return Position.MarketPosition == MarketPosition.Short && (CrossBelow(_emaMin, _emaMax, 1) && _pSar[0] > Closes[1][0]) && _canExtra; //&& EMA 15  i 5 oraz SAR ;

        }

        private bool firstTrailConditionShort()
        {
            if (status == "Short 2 Default" || status == "Short 1 Default" && status != "Extra Short")
            {
                return Position.MarketPosition == MarketPosition.Short && Close[0] <= _shortEntryPrice1 - (StopSwitchShort1 * TickSize) && status != "Short Stop Breakeven";
            }
            else
            {
                return false;
            }
        }


        private bool ShortCondition1()
        {
            _stochFast = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
            return (Position.MarketPosition == MarketPosition.Flat &&  _stochFast >= EntryFastStochValueShort && previousCandleRed()); //&& Closes[0][0] < _sma[0]; //|| (isDowntrend() && Position.MarketPosition == MarketPosition.Flat && _checkPointShort == true && _canTrade && _rsiEntry[0] <= EntryRsiValueShort - _thresholdShort - 1);
        }


        private bool LongCondition1()
        {
         _stochFast = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
          return (Position.MarketPosition == MarketPosition.Flat && _stochFast <= EntryFastStochValueLong && previousCandleGreen()); //&& Closes[0][0] > _sma[0]; // || (isUptrend() && Position.MarketPosition == MarketPosition.Flat && _checkPointLong == true &&  _canTrade && _rsiEntry[0] >= EntryRsiValueLong + _thresholdLong + 1 );
        }


        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (OrderFilled(order) && IsLongOrder1(order))
            {
                _longEntryPrice1 = averageFillPrice;
                _stopLossBaseLong = calculateStopLong(); 
                SetStopLoss("Long Entry1",CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Long Entry1", CalculationMode.Ticks, ProfitTarget);
                status = "Long 1 Default";
            }
            else if (OrderFilled(order) && IsLongOrder2(order))
            {
                SetStopLoss("Long Entry2", CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Long Entry2", CalculationMode.Ticks, ProfitTargetLong2);
            }
            else if (OrderFilled(order) && IsLongOrder3(order))
            {
                SetStopLoss("Long Entry3", CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Long Entry3", CalculationMode.Ticks, ProfitTargetLong3);
            }

            else if (OrderFilled(order) && IsLongOrder4(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopLong());
                SetProfitTarget("Long Entry4", CalculationMode.Ticks, ExtraTargetLong1);
            }

            else if (OrderFilled(order) && IsLongOrder5(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopLong());
                SetProfitTarget("Long Entry4", CalculationMode.Ticks, ExtraTargetLong2);
            }

            else if (OrderFilled(order) && IsLongOrder6(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopLong());
                SetProfitTarget("Long Entry4", CalculationMode.Ticks, ExtraTargetLong3);
            }

            else if (OrderFilled(order) && IsShortOrder1(order))
            {
                _shortEntryPrice1 = averageFillPrice;
                _stopLossBaseShort = calculateStopShort();
                SetStopLoss("Short Entry1", CalculationMode.Price, _stopLossBaseShort, false);
                SetProfitTarget("Short Entry1", CalculationMode.Ticks, ProfitTargetShort);
                status = "Short 1 Default";
            }
            else if (OrderFilled(order) && IsShortOrder2(order))
            {
                SetStopLoss("Short Entry2",CalculationMode.Price, _stopLossBaseShort, false);
                SetProfitTarget("Short Entry2", CalculationMode.Ticks, ProfitTargetShort2);
            }
            else if (OrderFilled(order) && IsShortOrder3(order))
            {
                SetStopLoss("Short Entry3", CalculationMode.Price, _stopLossBaseShort, false);
                SetProfitTarget("Short Entry3", CalculationMode.Ticks, ProfitTargetShort3);
            }
            else if (OrderFilled(order) && IsShortOrder4(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopShort());
                SetProfitTarget("Short Entry4", CalculationMode.Ticks, ExtraTargetShort1);
            }
            else if (OrderFilled(order) && IsShortOrder5(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopShort());
                SetProfitTarget("Short Entry4", CalculationMode.Ticks, ExtraTargetShort2);
            }
            else if (OrderFilled(order) && IsShortOrder6(order))
            {
                SetStopLoss(CalculationMode.Price, calculateStopShort());
                SetProfitTarget("Short Entry4", CalculationMode.Ticks, ExtraTargetShort3);
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

            return highestHigh;
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

        private bool previousCandleRed()
        {
            return Closes[1][0] <= Opens[1][0] && Closes[1][1] >= Opens[1][1];
        }

        private bool previousCandleGreen()
        {
            return Closes[1][0] >= Opens[1][0] && Closes[1][1] <= Opens[1][1];
        }



        private void AddIndicators()
        {
            _emaMin = EMA(BarsArray[1],EmaMinPeriod);
            _emaMax = EMA(BarsArray[1],EmaMaxPeriod);
            _sma = SMA(BarsArray[0], 100);
            _pSar = ParabolicSAR(0.02, 0.2, 0.02);
            AddChartIndicator(_emaMin);
            AddChartIndicator(_emaMax);
            AddChartIndicator(_pSar);
        }
    }
}

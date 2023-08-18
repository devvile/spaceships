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
    public class SputnikIV : Strategy
    {
        #region declarations
        private Indicator _sma;
        private Indicator _emaMin;
        private Indicator _emaMax;
        private Indicator _rsiEntry;
        private Indicator _pSar;
        private Order _longOneOrder;
        private Order _longTwoOrder;
        private Order _longThreeOrder;
        private Order _longFourOrder;


        private Order _shortOneOrder;
        private Order _shortTwoOrder;
        private Order _shortThreeOrder;
        private Order _shortFourOrder;
        private Order _longTargetOrder1;
        private double _longEntryPrice1;
        private double _shortEntryPrice1;
        private double _longStopSwitch1Ticks;
        private double _longStopSwitch2Ticks;
        private double _shortStopSwitch1Ticks;
        private double _shortStopSwitch2Ticks;
        private double _longStopLoss2;
        private double _stopLossBaseLong;
        private double _stopLossBaseShort;
        #endregion

        #region My Parameters
        private double _stopLoss = 28;
        private double _stopLossShort = 28;
        private double _profitTarget = 60;
        private double _profitTargetLong2 = 80;
        private double _profitTargetLong3 = 120;
        private double _extraTargetLong = 60;
        private double _profitTargetShort = 40;
        private double _profitTargetShort2 = 60;
        private double _profitTargetShort3 = 80;
        private double _extraTargetShort = 60;

        private int _entryRsiPeriod = 9;
        private int _entryRsiValueLong = 29;
        private int _entryRsiValueShort = 71;
        private int _baseStopMarginLong = 6;
        private int _baseStopMarginShort = 6;

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

        [Display(Name = "Entry RSI Period", GroupName = "General", Order = 0)]
        public int EntryRsiPeriod
        {
            get { return _entryRsiPeriod; }
            set { _entryRsiPeriod = value; }
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


        [Display(Name = "Entry RSI Value", GroupName = "LONGS", Order = 0)]
        public int EntryRsiValueLong
        {
            get { return _entryRsiValueLong; }
            set { _entryRsiValueLong = value; }
        }

        [Display(Name = "Initial Stop Loss Long", GroupName = "LONGS", Order = 0)]
        public double StopLoss
        {
            get { return _stopLoss; }
            set { _stopLoss = value; }
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

        [Display(Name = "Extra Target Long", GroupName = "LONGS", Order = 0)]
        public double ExtraTargetLong
        {
            get { return _extraTargetLong; }
            set { _extraTargetLong = value; }
        }

        [Display(Name = "Stop Switch Long Level 1", GroupName = "Targets", Order = 0)]
        public double StopSwitchLong1
        {
            get { return _longStopSwitch1Ticks; }
            set { _longStopSwitch1Ticks = value; }
        }


        [Display(Name = "Stop Switch Long Level 2", GroupName = "Targets", Order = 0)]
        public double StopSwitchLong2
        {
            get { return _longStopSwitch2Ticks; }
            set { _longStopSwitch2Ticks = value; }
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


        [Display(Name = "Entry RSI Value Short ", GroupName = "SHORTS", Order = 0)]
        public int EntryRsiValueShort
        {
            get { return _entryRsiValueShort; }
            set { _entryRsiValueShort = value; }
        }

        [Display(Name = "Stop Switch Short Level 1", GroupName = "SHORTS", Order = 0)]
        public double StopSwitchShort1
        {
            get { return _shortStopSwitch1Ticks; }
            set { _shortStopSwitch1Ticks = value; }
        }

        [Display(Name = "Stop Switch Short Level 2", GroupName = "SHORTS", Order = 0)]
        public double StopSwitchShort2
        {
            get { return _shortStopSwitch2Ticks; }
            set { _shortStopSwitch2Ticks = value; }
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

        [Display(Name = "Extra Target Short", GroupName = "SHORTS", Order = 0)]
        public double ExtraTargetShort
        {
            get { return _extraTargetShort; }
            set { _extraTargetShort = value; }
        }

        [Display(Name = "Initial Stop Loss Short", GroupName = "SHORTS", Order = 0)]
        public double StopLossShort
        {
            get { return _stopLossShort; }
            set { _stopLossShort = value; }
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





        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Sputnik IV";
                Name = "Sputnik IV";
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
            if (BarsInProgress == 0)
            {
                calculateStopLong();
                calculateStopShort();
            }


            if (BarsInProgress == 1)
            {
                Print("HAIKEN ASHI!");
                Print(Time[0]);
                Print("CLOSE" + Close[0]);
                Print("HIGH" + High[0]);
                Print("LOW" + Low[0]);
                Print("OPEN" + Open[0]);
                Print("RSI:");
                Print(_rsiEntry[0]);
                Print("Reset:");
                Print(Reset);
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
                    _canExtra = false;
                }


                /*
                if (firstTrailConditionLong()) //move to breakEven
                {
                    Print("Moving stop to Breakeven");
                    SetStopLoss(CalculationMode.Price, _longEntryPrice1 + (6 * TickSize));
                    status = "Long Stop Breakeven";
                }

                if (secondTrailConditionLong()) 
                {
                    Print("Moving stop to LEVEL 2 Long");
                    SetStopLoss(CalculationMode.Price, _longEntryPrice1 + (ProfitTarget * TickSize));
                    status = "Long Stop Second";
                }
                */
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
                    _canExtra = false;
                }

                /*
                if (firstTrailConditionShort()) //move to breakEven
                {
                    Print("Moving stop to Breakeven");
                    Print("Print TUTAJ KRUWA TUTAJ");
                    Print(_shortEntryPrice1);
                    Print("NEW LEVEL SL");
                    Print(_shortEntryPrice1 - (3 * TickSize));
                    status = "Short Stop Breakeven";
                    //                    SetStopLoss(CalculationMode.Price, _longEntryPrice1 + (6 * TickSize));
                      SetStopLoss(CalculationMode.Price, _shortEntryPrice1 - (3 * TickSize));
            
                }
       
                if (secondTrailConditionShort()) //move to breakEven
                {
                    Print("Moving stop to Level 2 Short");
                    SetStopLoss(CalculationMode.Price, _shortEntryPrice1 - (ProfitTargetShort * TickSize));
                    status = "Short Stop Second";
                    Draw.HorizontalLine(this, "Long Trail Stop Switch 2", _shortEntryPrice1 + (ProfitTargetShort * TickSize), Brushes.Blue);
                }
                */

            }

        }

        private bool firstTrailConditionLong()
        {
            if (status == "Long 2 Default" || status == "Long 1 Default")
            {
                return Position.MarketPosition == MarketPosition.Long && Close[BarNr] >= _longEntryPrice1 + (StopSwitchLong1 * TickSize) && status != "Long Stop Breakeven";
            }
            else
            {
                return false;
            }

        }

        private bool secondTrailConditionLong()
        {
            if (status == "Long Stop Breakeven")
            {
                return Position.MarketPosition == MarketPosition.Long && Close[0] >= _longEntryPrice1 + (StopSwitchLong2 * TickSize) && status != "Long Stop Second";
            }
            else
            {
                return false;
            }

        }

        private bool LongExtraCondition1()
        {
                return Position.MarketPosition == MarketPosition.Long && _canExtra && (CrossAbove(_emaMin, _emaMax,  1) && _pSar[0] < Closes[1][0]);//&& EMA 15  i 5 oraz SAR ;
        }

        private bool ShortExtraCondition1()
        {
                return Position.MarketPosition == MarketPosition.Short && _canExtra && (CrossBelow(_emaMin, _emaMax,  1) && _pSar[0] > Closes[1][0]); //&& EMA 15  i 5 oraz SAR ;

        }

        private bool firstTrailConditionShort()
        {
            if (status == "Short 2 Default" || status == "Short 1 Default")
            {
                return Position.MarketPosition == MarketPosition.Short && Close[0] <= _shortEntryPrice1 - (StopSwitchShort1 * TickSize) && status != "Short Stop Breakeven";
            }
            else
            {
                return false;
            }
        }

        private bool secondTrailConditionShort()
        {
            if (status == "Short Stop Breakeven")
            {
                return Position.MarketPosition == MarketPosition.Short && Close[0] <= _shortEntryPrice1 - (StopSwitchShort2 * TickSize) && status != "Short Stop Second";
            }
            else
            {
                return false;
            }
        }

        private bool ShortCondition1()
        {
            return (Position.MarketPosition == MarketPosition.Flat && _rsiEntry[1] >= EntryRsiValueShort && previousCandleRed()); //&& Closes[0][0] < _sma[0]; //|| (isDowntrend() && Position.MarketPosition == MarketPosition.Flat && _checkPointShort == true && _canTrade && _rsiEntry[0] <= EntryRsiValueShort - _thresholdShort - 1);
        }


        private bool LongCondition1()
        {

            return (Position.MarketPosition == MarketPosition.Flat && _rsiEntry[1] <= EntryRsiValueLong && previousCandleGreen()); //&& Closes[0][0] > _sma[0]; // || (isUptrend() && Position.MarketPosition == MarketPosition.Flat && _checkPointLong == true &&  _canTrade && _rsiEntry[0] >= EntryRsiValueLong + _thresholdLong + 1 );
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
                SetTrailStop("Long Entry4", CalculationMode.Price, calculateStopLong(), false);
                SetProfitTarget("Long Entry4", CalculationMode.Ticks, ExtraTargetLong);
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
                SetTrailStop("Short Entry4", CalculationMode.Price, calculateStopShort(), false);
                SetProfitTarget("Short Entry4", CalculationMode.Ticks, ExtraTargetShort);
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
            _emaMin = EMA(BarsArray[1],EmaMinPeriod);
            _emaMax = EMA(BarsArray[1],EmaMaxPeriod);
            _sma = SMA(BarsArray[0], 100);
            _pSar = ParabolicSAR(0.02, 0.2, 0.02);
            AddChartIndicator(_emaMin);
            AddChartIndicator(_emaMax);
            AddChartIndicator(_pSar);
            _rsiEntry = RSI(BarsArray[1],EntryRsiPeriod, 1);
            AddChartIndicator(_rsiEntry);
        }
    }
}

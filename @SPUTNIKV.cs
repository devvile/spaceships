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
    public class SputnikV : Strategy
    {
        #region declarations
        private Indicator _sma;
        private Indicator _emaMin;
        private Indicator _emaMax;
        private Indicator _emaMinDay;
        private Indicator _emaMaxDay;
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


        private int LotSize1;
        private int LotSize2;
        private int LotSize3;
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

        private int _heikenPeriod = 240;
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


        private int _emaMinPeriodDay = 5;
        private int _emaMaxPeriodDay = 15;

        private int _emaMinutePeriodMin = 60;


        private int lastTrades = 0;    // This variable holds our value for how profitable the last trades were. Splitted position are individually counted.
        private int priorNumberOfTrades = 0;    // This variable holds the number of trades taken. It will be checked every OnBarUpdate() to determine when a trade has closed.
        private int priorSessionTrades = 0;

        private bool _canExtra = true; // Enables Extra posistions
        private bool _useEma = true;
        private bool _useLongs = true;
        private bool _useShorts = true;
        private bool _reset = true;
        private bool _log = false;
        private bool _start = false;
        private bool _useHeikenAshi = true;

        private bool _doingGood = true;

        private int _lotSize1 = 2;
        private int _lotSize2 = 4;
        private int _lotSize3 = 1;
        private int _consecutiveLosses = 4;
        private int _extraSize = 3;
        private int nr;
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

        [Display(Name = "Consecutive Losses Block number", GroupName = "General", Order = 0)]
        public int ConsecutvieLosses
        {
            get { return _consecutiveLosses; }
            set { _consecutiveLosses = value; }
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
        public int UserLotSize1
        {
            get { return _lotSize1; }
            set { _lotSize1 = value; }
        }

        [Display(Name = "Position Size 2", GroupName = "Position Management", Order = 0)]
        public int UserLotSize2
        {
            get { return _lotSize2; }
            set { _lotSize2 = value; }
        }


        [Display(Name = "Position Size 3", GroupName = "Position Management", Order = 0)]
        public int UserLotSize3
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


        [Display(Name = "Use Heiken Ashi Day filter", GroupName = "Filters", Order = 0)]
        public bool UseHeikenDayFilter
        {
            get { return _useHeikenAshi; }
            set { _useHeikenAshi = value; }
        }
        [Display(Name = "Use Heiken Ashi filter minutes period", GroupName = "Filters", Order = 0)]
        public int HeikenPeriod
        {
            get { return _heikenPeriod; }
            set { _heikenPeriod = value; }
        }

        #endregion

        #region EMA

        [Display(Name = "Use EMA", GroupName = "EMA", Order = 0)]
        public bool UseEma
        {
            get { return _useEma; }
            set { _useEma = value; }
        }


        [Display(Name = "EXTRA EMA Min PERIOD", GroupName = "EMA", Order = 3)]
        public int EmaMinPeriod
        {
            get { return _emaMinPeriod; }
            set { _emaMinPeriod = value; }
        }


        [Display(Name = "EXTRA EMA Max PERIOD", GroupName = "EMA", Order = 4)]
        public int EmaMaxPeriod
        {
            get { return _emaMaxPeriod; }
            set { _emaMaxPeriod = value; }
        }

        [Display(Name = "DAY EMA Min PERIOD", GroupName = "EMA", Order = 1)]
        public int EmaMinPeriodDay
        {
            get { return _emaMinPeriodDay; }
            set { _emaMinPeriodDay = value; }
        }


        [Display(Name = "DAY EMA Max PERIOD", GroupName = "EMA", Order = 2)]
        public int EmaMaxPeriodDay
        {
            get { return _emaMaxPeriodDay; }
            set { _emaMaxPeriodDay = value; }
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
                Description = @"Sputnik V fith filter";
                Name = "Sputnik V";
                Calculate = Calculate.OnBarClose;
                BarsRequiredToTrade = 20;
                AddPlot(Brushes.Blue, "Upper");
                AddPlot(Brushes.Green, "Lower");

            }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                AddHeikenAshi("MES 09-23", BarsPeriodType.Minute, 1, MarketDataType.Last);
                AddDataSeries(BarsPeriodType.Day, 1);
                AddHeikenAshi("MES 09-23", BarsPeriodType.Day, 1, MarketDataType.Last);
                AddHeikenAshi("MES 09-23", BarsPeriodType.Minute, HeikenPeriod, MarketDataType.Last);
            }

            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();
                AddIndicators();
            }
        }

        protected override void OnBarUpdate()
        {

            if (CurrentBars[0] <= BarsRequiredToTrade || CurrentBars[1] <= BarsRequiredToTrade || CurrentBars[2] <= BarsRequiredToTrade)  return;



            if (Position.MarketPosition == MarketPosition.Flat)
            {
                status = "Flat";

            }

            if (Bars.IsFirstBarOfSession)
            {
                lastTrades = 0;
                priorSessionTrades = SystemPerformance.AllTrades.Count;
            }

            if (BarsInProgress == 1 && _log)
            {
                
                Print("HAIKEN ASHI!");
                Print(Time[0]);
                Print("CLOSE" + Close[0]);
                Print("HIGH" + High[0]);
                Print("LOW" + Low[0]);
                Print("OPEN" + Open[0]);
                Print("~~~~~~~~~~~~~~");
            }

            if(BarsInProgress == 2)
            {
                filterHeikenAshi();
            };

            checkForLosingTrades();
            /*
            if (Position.MarketPosition == MarketPosition.Flat)
                {
                    _canExtra = true;
                    _doingGood= true;
                }
        */
            ResetStrategy();
            if (lastTrades != -1 * ConsecutvieLosses * 3)
            {
                _canExtra = true;
                _doingGood = true;
                LotSize1 = UserLotSize1;
                LotSize2 = UserLotSize2;
                LotSize3 = UserLotSize3;
           //     nr = rnd.Next();
           //     string tag = nr.ToString();
                //      Draw.ArrowUp(this, "tag" + tag, true, 0, Low[0] - TickSize, Brushes.White);
            }
            else
            {
                _canExtra = false;
                _doingGood = false;
                LotSize1 = 1;
                LotSize2 = 1;
                LotSize3 = 1;
            //    nr = rnd.Next();
            //    string tag = nr.ToString();
                //        Draw.ArrowUp(this, "tag" + tag, true, 0, Low[0] - TickSize, Brushes.Red);
            }

            if (UseLongs)
            {
                if (LongCondition1())
                {
                    _longOneOrder = EnterLong(LotSize1,  "Basic Long Entry1");


                    if (_doingGood)
                    {
                        _longTwoOrder = EnterLong(LotSize2, "Basic Long Entry2");
                        _longThreeOrder = EnterLong(LotSize3, "Basic Long Entry3");
                    }

                }
                if (LongExtraCondition1())
                {
                    _longFourOrder = EnterLong(ExtraSize, "Extra Long Entry1");
                    _longFiveOrder = EnterLong(ExtraSize, "Extra Long Entry2");
                    _longSixOrder = EnterLong(ExtraSize, "Extra Long Entry3");
                    status = "Extra Long";
                }


                if (firstTrailConditionLong()) 
                {
                    SetStopLoss(CalculationMode.Price, _longEntryPrice1 + (1 * TickSize));
                    status = "Long Stop Breakeven";
                }
            }

            if (UseShorts)
            {
                if (ShortCondition1())
                {

                    _shortOneOrder = EnterShort(LotSize1, "Basic Short Entry1");
      //               _shortTwoOrder = EnterShort(LotSize2, "Basic Short Entry2");
                    if (_doingGood)
                    {
                        _shortTwoOrder = EnterShort(LotSize2, "Basic Short Entry2");
                        _shortThreeOrder = EnterShort(LotSize3, "Basic Short Entry3");
                    }
                }

                if (ShortExtraCondition1())
                {
                    _shortFourOrder = EnterShort(ExtraSize, "Extra Short Entry1");
                    _shortFiveOrder = EnterShort(ExtraSize, "Extra Short Entry2");
                    _shortSixOrder = EnterShort(ExtraSize, "Extra Short Entry3");
                    status = "Extra Short";
                }

                if (firstTrailConditionShort())
                { 
                    Print(_shortEntryPrice1);
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
            return (Position.MarketPosition == MarketPosition.Flat && _stochFast >= EntryFastStochValueShort && previousCandleRed()) && isDowntrend(); // && Position.MarketPosition == MarketPosition.Flat && _checkPointShort == true && _canTrade && _rsiEntry[0] <= EntryRsiValueShort - _thresholdShort - 1);
        }


        private bool LongCondition1()
        {
            _stochFast = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
            return (Position.MarketPosition == MarketPosition.Flat && _stochFast <= EntryFastStochValueLong && previousCandleGreen()) && isUptrend(); // || (isUptrend() && Position.MarketPosition == MarketPosition.Flat && _checkPointLong == true &&  _canTrade && _rsiEntry[0] >= EntryRsiValueLong + _thresholdLong + 1 );
        }

        #region onOrderUpdate
        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (OrderFilled(order) && IsLongOrder1(order))
            {
                _longEntryPrice1 = averageFillPrice;
                _stopLossBaseLong = calculateStopLong(); 
                SetStopLoss("Basic Long Entry1",CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Basic Long Entry1", CalculationMode.Ticks, ProfitTarget);
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
                SetProfitTarget("Basic Short Entry1", CalculationMode.Ticks, ProfitTargetShort);
                status = "Short 1 Default";
            }
            else if (OrderFilled(order) && IsShortOrder2(order))
            {
                SetStopLoss("Basic Short Entry2",CalculationMode.Price, _stopLossBaseShort, false);
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
        #endregion

        #region Stop Calculation
        private double calculateStopLong()
        {
            List<double> lows = new List<double> { Lows[0][1], Lows[0][2], Lows[0][3], Lows[0][4]};
            lows.Sort();
            double lowestLow = lows[0];
            double baseStopLoss = lowestLow - BaseStopMarginLong * TickSize;
            if (_log)
            {
                Print("~~~~~~~~~");
                Print("LOWEST LOW for Stop Loss:");
                Print(lowestLow);
                Print("Stop Loss Set at:");
                Print(baseStopLoss);
                Print("~~~~~~~~~");
            }


            return baseStopLoss;
        }

        private double calculateStopShort()
        {
            List<double> highs = new List<double> { Highs[0][1], Highs[0][2], Highs[0][3], Highs[0][4] };
            highs.Sort();
            highs.Reverse();
            double highestHigh = highs[0];
            double baseStopLoss = highestHigh + BaseStopMarginShort * TickSize;
            if (_log)
            {
                Print("~~~~~~~~~");
                Print("Highest High for Stop Loss:");
                Print(highestHigh);
                Print("Stop Loss Set at:");
                Print(baseStopLoss);
                Print("~~~~~~~~~");
            }

            return highestHigh;
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

        private void checkForLosingTrades()
        {
            if ((SystemPerformance.AllTrades.Count - priorSessionTrades) >= (ConsecutvieLosses * 3) && SystemPerformance.AllTrades.Count != priorNumberOfTrades)
            {
                // Reset the counter.
                lastTrades = 0;

                // Set the new number of completed trades.
                priorNumberOfTrades = SystemPerformance.AllTrades.Count;
                // Loop through the last three trades and check profit/loss on each.
                for (int idx = 1; idx <= (ConsecutvieLosses * 3); idx++)
                {
                    /* The SystemPerformance.AllTrades array stores the most recent trade at the highest index value. If there are a total of 10 trades,
					   this loop will retrieve the 10th trade first (at index position 9), then the 9th trade (at 8), then the 8th trade. */
                    Trade trade = SystemPerformance.AllTrades[SystemPerformance.AllTrades.Count - idx];

                    /* If the trade's profit is greater than 0, add one to the counter. If the trade's profit is less than 0, subtract one.
						This logic means break-even trades have no effect on the counter. */
                    if (trade.ProfitCurrency > 0)
                    {
                        lastTrades++;
                    }

                    else if (trade.ProfitCurrency < 0)
                    {
                        lastTrades--;
                    }
                }
            }
        }

        private void ResetStrategy() 
        {
            if (Position.MarketPosition == MarketPosition.Short && Reset)
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
        }

        private bool isUptrend()
        {
            if (UseEma)
            {
                return _emaMinDay[0] > _emaMaxDay[0] ;
            }
            else
            {
                return true;
            }
        }

        private bool isDowntrend()
        {
            if (UseEma)
            {
                return  _emaMinDay[0] < _emaMaxDay[0];
            }
            else
            {
                return true;
            }
        }
    
        private bool previousCandleRed()
        {
            return Closes[1][0] <= Opens[1][0] && Closes[1][1] >= Opens[1][1];
        }

        private bool previousCandleGreen()
        {
            return Closes[1][0] >= Opens[1][0] && Closes[1][1] <= Opens[1][1];
        }


        private void filterHeikenAshi()
        {
            if (_useHeikenAshi)
            {
                if(Closes[3][0] <= Opens[3][0] && Closes[3][1] >= Opens[3][1]) //closing candle red after previous green on D1;
                {
                    UseLongs = false;
                    UseShorts = true;
                }
                else if ( Closes[3][0] >= Opens[3][0] && Closes[3][1] <= Opens[3][1] ) ////closing candle green after previous red on D1;
                {
                    UseLongs = true;
                    UseShorts = false;
                }
            }
        }

        private void AddIndicators()
        {
            _emaMin = EMA(BarsArray[1],EmaMinPeriod);
            _emaMax = EMA(BarsArray[1],EmaMaxPeriod);
            _emaMaxDay = EMA(BarsArray[2], EmaMaxPeriodDay);
            _emaMinDay = EMA(BarsArray[2], EmaMinPeriodDay);
            _sma = SMA(BarsArray[0], 100);
            _pSar = ParabolicSAR(0.02, 0.2, 0.02);
            AddChartIndicator(_emaMin);
            AddChartIndicator(_emaMax);
      //      AddChartIndicator(_emaMinDay);
    //        AddChartIndicator(_emaMaxDay);
            AddChartIndicator(_pSar);
         //   PlotBrushes[0][0] = Brushes.Blue;
        }
    }
}

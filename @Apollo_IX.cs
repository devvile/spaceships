#region Using declarations
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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public class Apollo_IX : Strategy
    {
        #region declarations


        private Indicator _rsi;
        private bool _canTrade;
        private int _tradingTimeStart;
        private int _tradingTimeEnd;
        private double _atrFilterValue;
        private double _vixFilterValue;

        #region Signal
        private string signal;
        #endregion

        #region Indicators
        private Indicator _aroon;
        private Indicator _stoch;
        private Indicator _ha;
        private Indicator _atr;
        private Indicator _Vix;
        private Indicator _iczimoku;
        private double _stochFast;

        private int _atrPeriod;
        private int _aroonPeriod;
        private int _stochRsiPeriod;
        private double _atrValue;
        #endregion

        #region Position Management Declarations

        private int _weakEntrySize;
        private int _strongEntrySize;
        private int _runnerSize;
        private int _barsToCheck;
        private int _maxLossMargin;
        private string status;
        private double stopLossPrice;
        private int _minTicksTarget;
        #endregion


        #region Levels Declarations

        double todayGlobexLow;
        double todayGlobexHigh;
        double yesterdayGlobexLow;
        double yesterdayGlobexHigh;
        int globexStartTime;
        double todayRTHLow;
        double todayRTHHigh;
        double yesterdayRTHLow;
        double yesterdayRTHHigh;
        int rthStartTime = 153000;
        int rthEndTime = 215900;
        double lastWeekHigh;
        double lastWeekLow;
        double thisWeekHigh;
        double thisWeekLow;
        double IBLow;
        double IBHigh;
        int IbEndTime;


       int  _ticksToTarget;
       PriceLevel[] keyLevels = { };
        PriceLevel[] keyLevelsInDistanceLong = { };
        PriceLevel[] keyLevelsInDistanceShort = { };



        #endregion

        #region Orders
        private Order _longBaseOrder;
        private Order _longRunnerOrder;
        private Order _shortBaseOrder;
        private Order _shortRunnerOrder;
        #endregion

        #region Longs Declarations
        private bool _useLongs;
        private int _rsiEntryLong;
        private double _longEntryPrice1;
        private int _longStopMargin;
        private int _profitLongRunner;

        private bool _longTargetInRange;
        private double _longAtrRatio;

        private double longClosestLevelPrice;
        private int _trailThresholdLong;
        private int _trailThresholdLong2;
        private int _trail2LevelLong;
        private int _minTicksTargetLong;

        #endregion

        #region Shorts Declarations
        private bool _useShorts;
        private int _rsiEntryShort;
        private double _shortEntryPrice1;
        private int _shortStopMargin;
        private bool _shortTargetInRange;
        private int _profitShortRunner;
        private int  _profitShortMain;
        private int _trailThresholdShort;
        private int _trailThresholdShort2;
        private int _trail2LevelShort;
        private int _minTicksTargetShort;
        private double _shortAtrRatio;

        #endregion

        #endregion
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"APOLLO CAN TO THE MOON!";
                Name = "Apollo IX";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 2;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 5;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0.8;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
                signal = "No Signal";

                _ticksToTarget = 2;

               //Shorts 

               _useShorts = true;
                _rsiEntryShort = 85;
                _shortStopMargin =16;
                _profitShortMain = 20;
                _shortAtrRatio = 0.8;
                //Longs

                _useLongs = true;
                _rsiEntryLong = 20;
                _longStopMargin = 20;
                _longAtrRatio = 0.8;

                _tradingTimeStart = 153000;
                _tradingTimeEnd = 214000;

                //filters
                _aroonPeriod = 5;
                _atrPeriod = 12;
                _stochRsiPeriod = 14;
                _atrFilterValue = 4.25;
                //LEVELS Detection



                #region Position Management defaults

                _weakEntrySize = 2;
                _strongEntrySize = 4;
                _runnerSize = 1;
                _maxLossMargin = 64;
                _minTicksTarget = 10;
                #endregion


                #region Level Detection defaults
                todayGlobexLow = 0;
                todayGlobexHigh = 0;
                yesterdayGlobexLow = 0;
                yesterdayGlobexHigh = 0;
                globexStartTime = 100;
                todayRTHLow = 0;
                todayRTHHigh = 0;
                yesterdayRTHLow = 0;
                yesterdayRTHHigh = 0;
                rthStartTime = 153000;
                rthEndTime = 215930;
                lastWeekHigh = 0;
                lastWeekLow = 0;
                thisWeekHigh = 0;
                thisWeekLow = 0;

                IBLow = 0;
                IBHigh = 0;
                IbEndTime = 163000;
                #endregion


                IsInstantiatedOnEachOptimizationIteration = true;
            }
            else if (State == State.Configure)
            {


                AddDataSeries(BarsPeriodType.Minute, 1);
                AddDataSeries(BarsPeriodType.Minute, 4);
                AddDataSeries(BarsPeriodType.Minute, 16); //Aroon, ATR
                AddDataSeries(BarsPeriodType.Day, 1);
                AddDataSeries(BarsPeriodType.Week, 1);
            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();
                AddIndicators();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade || CurrentBars[2] < BarsRequiredToTrade || CurrentBars[3] < BarsRequiredToTrade || CurrentBars[4] < 10|| CurrentBars[5] < 1)
                return;


            CalculateTradeTime();   //enabling trading in strictly declared hours
            CalculateLevels();    // calculation of price-hour levels

            //      CalculateLevelsDistance(); // calculation of distance from price to levels


            if (Position.MarketPosition != MarketPosition.Flat && ToTime(Time[0]) >= rthEndTime) // Exit all positions on session close
            {
                ExitLong();
                ExitShort();
              //  ShowLevels(keyLevels);
            }

            if (_canTrade && BarsInProgress == 1 && _Vix[0] > VixFilterValue)
            {
                signal = calculateSignal();
                _atrValue = _atr[0];
                Trail();
                AdjustStop();
                CalculateLevelsDistance(Close[0]);
                IchimokuMode();
            }
            //Add your custom strategy logic here.
        }

        private void ShowLevels(PriceLevel[] keyLevels)
        {
            foreach (PriceLevel element in keyLevels)
            {
                Print("xxxxxxxxxxxxx");
                Print(Time[0]);
                Print(element.Name);
                Print(element.Price);
                Print("xxxxxxxxxxxxx");
            }
        }

        private void Trail()
        {
            if (Close[0] >= Position.AveragePrice + (TrailThresholdLong * TickSize) && Position.MarketPosition == MarketPosition.Long && status != "Breakeven2 Short" && status != "Breakeven2 Long")
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


        static double getClosestNumber(PriceLevel[] arr, double entryPrice, double targetPrice)
        {
            double diff;
            double smallestDifference=0;
            double closestLevel;
            foreach (PriceLevel levelPrice in arr)
            {
                diff = Math.Abs(levelPrice.Price - entryPrice);
                if (diff < smallestDifference)
                {
                    smallestDifference = diff;
                    closestLevel = levelPrice.Price;
                }
            }
            return smallestDifference;
        }


            private void CalculateTradeTime()
        {

            if ((ToTime(Time[0]) >= _tradingTimeStart && ToTime(Time[0]) < _tradingTimeEnd))
            {
                _canTrade = true;
            }
            else
            {
                _canTrade = false;
            }
        }


        private void CalculateLevels()
        {
                       //     Print("Calculating Key Levels...");
    


                            //Calculating Levels aand assingment to  keyLevels array;

            #region Levels calculation

            if (Bars.IsFirstBarOfSession && BarsInProgress == 1)
            {
                if (thisWeekHigh == 0)
                {
                    thisWeekHigh = High[0];
                }
                if (thisWeekLow == 0)
                {
                    thisWeekLow = Low[0];
                }

            }

            if (BarsInProgress == 1)
            {

                if (High[0] > thisWeekHigh)
                {
                    thisWeekHigh = High[0];
                };

                if (Low[0] < thisWeekLow)
                {
                    thisWeekLow = Low[0];
                };

                if (ToTime(Time[0]) == globexStartTime)
                {
                    yesterdayGlobexLow = todayGlobexLow;
                    yesterdayGlobexHigh = todayGlobexHigh;
                    yesterdayRTHLow = todayRTHLow;
                    yesterdayRTHHigh = todayRTHHigh;
                    todayGlobexLow = Low[0];
                    todayGlobexHigh = High[0];
                }

                else if (ToTime(Time[0]) == rthStartTime)
                {

                    todayRTHLow = Low[0];
                    todayRTHHigh = High[0];
                    IBLow = Low[0];
                    IBHigh = High[0];
                }

                if (isGlobex(ToTime(Time[0])))
                {
                    if (High[0] > todayGlobexHigh)
                    {
                        todayGlobexHigh = High[0];
                    }
                    else if (Low[0] < todayGlobexLow)
                    {
                        todayGlobexLow = Low[0];
                    }
                }  // Today Globex high/low

                if (isRTH(ToTime(Time[0])))
                {
                    if (High[0] > todayRTHHigh)
                    {
                        todayRTHHigh = High[0];
                    }
                    else if (Low[0] < todayRTHLow)
                    {
                        todayRTHLow = Low[0];
                    }
                }  // Today RTH high/low


                if (isIB(ToTime(Time[0])))
                {
                    if (High[0] > IBHigh)
                    {
                        IBHigh = High[0];
                    }
                    else if (Low[0] < IBLow)
                    {
                        IBLow = Low[0];
                    }
                }  // Today IB high/low
            }

            if (BarsInProgress == 4 && CurrentBars[2] >= 1) //16
            {
                lastWeekHigh = Highs[4][0];
                lastWeekLow = Lows[4][0];
                thisWeekHigh = Highs[4][0];
                thisWeekLow = Lows[4][0];
            }
            #endregion

            PriceLevel[] keyLevelsFilled = new PriceLevel[]
            {
              new PriceLevel("Today Globex High", todayGlobexHigh,0,false),
              new PriceLevel("Today Globex Low", todayGlobexLow,0,false),
              new PriceLevel("Yesterday Globex High", yesterdayGlobexHigh,0,false),
              new PriceLevel("Yesterday Globex Low", yesterdayGlobexLow,0,false),
              new PriceLevel("Today RTH High", todayRTHHigh,0,false),
              new PriceLevel("Today RTH Low", todayRTHLow,0,false),
              new PriceLevel("Yesterdat RTH High", yesterdayRTHHigh,0,false),
              new PriceLevel("YesterDay RTH Low", yesterdayRTHLow,0,false),
              new PriceLevel("IB High", IBHigh,0,false),
              new PriceLevel("IB Low", IBLow,0,false),
              new PriceLevel("Last Week High", lastWeekHigh,0,false),
              new PriceLevel("Last Week Low", lastWeekLow,0,false),
              new PriceLevel("This Week High", thisWeekHigh,0,false),
              new PriceLevel("This Week Low", thisWeekLow,0,false),
              new PriceLevel("VWAP", VWAP(BarsArray[1]).PlotVWAP[0],0,false)
            };

            keyLevels = keyLevelsFilled;
        }

        //STOP LOSS

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
                highs.Add(Highs[1][i]);

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
               SetStopLoss(CalculationMode.Ticks, Trail2LevelLong);
            }
            else if (status == "Breakeven2 Short")
            {
                SetStopLoss(CalculationMode.Ticks, Trail2LevelShort);
            }
        }

        #region LevelsTimeFunctions
        private bool isGlobex(int time)
        {
            if (time >= globexStartTime && time <= rthStartTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool isRTH(int time)
        {
            if (time >= rthStartTime && time <= rthEndTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private bool isIB(int time)
        {
            if (time >= rthStartTime && time <= IbEndTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion





        private void CalculateLevelsDistance( double currentPrice)
        {
            _shortTargetInRange = false;
            _longTargetInRange = false;


            //Long
            
          
            List<PriceLevel> levelsAbovePrice = new List<PriceLevel>();

            foreach (PriceLevel level in keyLevels)
            {
                if ((level.Price - currentPrice) >= 0)
                {
                    levelsAbovePrice.Add(level);
                };
            };


            levelsAbovePrice = new List<PriceLevel>( levelsAbovePrice.OrderBy(level => level.Price));
            PriceLevel closestLevel = levelsAbovePrice.First(); //long
            double distance =   closestLevel.Price - currentPrice;



            if (distance > MinTicksTargetLong * TickSize)
            {
                _longTargetInRange = true;
            }
            else
            {
                _longTargetInRange = false;

            }

            //Short

            List<PriceLevel> levelsBelowPrice = new List<PriceLevel>();

            foreach (PriceLevel level in keyLevels)
            {
                if ((currentPrice >= level.Price))
                {
                    levelsBelowPrice.Add(level);
                };
            };

            levelsBelowPrice = new List<PriceLevel>(levelsBelowPrice.OrderBy(level => level.Price));
            PriceLevel closestBelowLevel = levelsBelowPrice.Last(); //Short
            double shortDistance = currentPrice - closestBelowLevel.Price;


            Print("xxxxxxxxxxxxxxxxxxxxxx");
            Print("Current Price is " + currentPrice);
            Print("Closest Level is: " + closestBelowLevel.Name);
            Print("Closest Level is: " + closestBelowLevel.Price);
            Print("Distance is " + shortDistance);
            Print("xxxxxxxxxxxxxxxxxxxxxx");


            if (shortDistance >  MinTicksTargetShort * TickSize)
            {
                _shortTargetInRange = true;
            }
            else
            {
                _shortTargetInRange = false;

            }
        }



        //Positions

        private bool noPositions()
        {
            return Position.MarketPosition == MarketPosition.Flat;
        }

        //Trigger

        private bool previousCandleRed()
        {
            return HeikenAshi8(BarsArray[1]).HAOpen[0] > HeikenAshi8(BarsArray[1]).HAClose[0] && HeikenAshi8(BarsArray[1]).HAOpen[1] < HeikenAshi8(BarsArray[1]).HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8(BarsArray[1]).HAOpen[0] < HeikenAshi8(BarsArray[1]).HAClose[0] && HeikenAshi8(BarsArray[1]).HAOpen[1] > HeikenAshi8(BarsArray[1]).HAClose[1];
        }

        private bool stochRsiEntry(int entryValue, string positionType)
        {
            _stochFast = StochRSIMod2NT8(BarsArray[1], stochRsiPeriod, 3, 3, 18).SK[1];
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

        // Filters

        private bool IsAroonUptrend()
        {
            return Aroon(BarsArray[3], aroonPeriod).Up[0] > 70;
        }

        private bool IsAroonDowntrend()
        {
            return Aroon(BarsArray[3], aroonPeriod).Down[0] > 70;
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

        //Signal

        private string calculateSignal()
        {
            string signalType = "No Signal";
            double recentSpanA = ApolloIchimoku(BarsArray[1], 9, 26, 52, 26).SpanALine[26];
            double recentSpanB = ApolloIchimoku(BarsArray[1], 9, 26, 52, 26).SpanBLine[26];
            double conversionLine = ApolloIchimoku(BarsArray[1], 9, 26, 52, 26).ConversionLine[0];
            double baseline = ApolloIchimoku(BarsArray[1], 9, 26, 52, 26).BaseLine[0];
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

        private void IchimokuMode()
        {

            signal = calculateSignal();
      //      Trail();

            if (signal != "No Singal" && applyFilters())
            {
                if (UseLongs && signal != "Strong Short" && signal != "Weak Short" && noPositions() && previousCandleGreen() && stochRsiEntry(RsiEntryLong, "Long") && _longTargetInRange)
                {
                    if (signal == "Strong Long")
                    {
                        _longBaseOrder = EnterLong(strongEntrySize, "Long Main");
                        _longRunnerOrder = EnterLong(runnerSize, "Long Runner");
                    }
                    else if (signal == "Weak Long")
                    {
                        _longBaseOrder = EnterLong(weakEntrySize, "Long Main");
                    }

                }
                else if (UseShorts && signal != "Strong Long" && signal != "Weak Long" && noPositions() && previousCandleRed() && stochRsiEntry(RsiEntryShort, "Short") && _shortTargetInRange)
                {
                    if (signal == "Strong Short")
                    {

                        _shortBaseOrder = EnterShort(strongEntrySize, "Short Main");
                        _shortRunnerOrder = EnterShort(runnerSize, "Short Runner");
                    }
                    else if (signal == "Weak Short")
                    {
                        _shortBaseOrder = EnterShort(weakEntrySize, "Short Main");
                    }
                }
            }

        }


        public class PriceLevel
        {

            public string Name { get; set; }
            public double Price { get; set; }
            public double Diff { get; set; }
            public bool InRange { get; set; }
            public PriceLevel(string name, double price, double difference, bool isInRange)
            {
                Name = name;
                Price = price;
                Diff = difference;
                InRange = isInRange;
            }
        }

        /*
        public class LevelsApp{
            PriceLevel[] PriceLevels = new PriceLevel[]{ };

            private RsiTest parent;
            public LevelsApp( RsiTest instance)
            {
                parent = instance;
            }

           

            public void MyMethod()
            {
            //    parent.Print("I just got executed!");
            }

            public void setDupa()
            {
              //  parent.dupa = 5;
            }

        }*/

        //Order Update

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (OrderFilled(order) && IsLongBaseOrder(order))
            {
                _longEntryPrice1 = averageFillPrice;
                stopLossPrice = calculateStopLong();
                SetProfitTarget("Long Main", CalculationMode.Price, _longEntryPrice1 + _atrValue * LongAtrRatio);
                status = "Long Default";
            }

            else if (OrderFilled(order) && IsLongRunnerOrder(order))
            {
                SetProfitTarget("Long Runner", CalculationMode.Ticks, ProfitLongRunner);
            }

            else if (OrderFilled(order) && IsShortBaseOrder(order))
            {
                _shortEntryPrice1 = averageFillPrice;
                stopLossPrice = calculateStopShort();
                status = "Short Default";
                SetProfitTarget("Short Main", CalculationMode.Ticks, ProfitShortMain);
         //       SetProfitTarget("Short Main", CalculationMode.Price, _shortEntryPrice1 - _atrValue * ShortAtrRatio);
            }

            else if (OrderFilled(order) && IsShortRunnerOrder(order))
            {
                SetProfitTarget("Short Runner", CalculationMode.Ticks, ProfitShortRunner);
            }

        }


        #region Orders Conditions

        private bool IsLongBaseOrder(Order order)
        {
            return order == _longBaseOrder;
        }

        private bool IsLongRunnerOrder(Order order)
        {
            return order == _longRunnerOrder;
        }


        private bool IsShortBaseOrder(Order order)
        {
            return order == _shortBaseOrder;
        }


        private bool IsShortRunnerOrder(Order order)
        {
            return order == _shortRunnerOrder;
        }

        private bool OrderFilled(Order order)
        {
            return order.OrderState == OrderState.Filled;
        }

        #endregion

        private void AddIndicators()
        {
            _ha = HeikenAshi8(BarsArray[1]);
            _aroon = Aroon(BarsArray[2], 5);
            _stoch = StochRSIMod2NT8(BarsArray[1], stochRsiPeriod, 3, 3, 18);
            _iczimoku = ApolloIchimoku(BarsArray[1], 9, 26, 52, 26);
            AddChartIndicator(_ha);
            AddChartIndicator(_stoch);
            AddChartIndicator(_aroon);
            AddChartIndicator(_iczimoku);
            _atr = ATR(BarsArray[3], atrPeriod);
            AddChartIndicator(_atr);
            _Vix = WVF(BarsArray[4]);
        }


        // PARAMETERES DEFINITION


        #region Position Management

        [Display(Name = "Bars to Check", GroupName = "Position Management", Order = 0)]
        public int BarsToCheck
        {
            get { return _barsToCheck; }
            set { _barsToCheck = value; }
        }

        [Display(Name = "Strong Entry Size", GroupName = "Position Management", Order = 0)]
        public int strongEntrySize
        {
            get { return _strongEntrySize; }
            set { _strongEntrySize = value; }
        }

        [Display(Name = "Weak Entry Size", GroupName = "Position Management", Order = 0)]
        public int weakEntrySize
        {
            get { return _weakEntrySize; }
            set { _weakEntrySize = value; }
        }

        [Display(Name = "Runner Size", GroupName = "Position Management", Order = 0)]
        public int runnerSize
        {
            get { return _runnerSize; }
            set { _runnerSize = value; }
        }

        [Display(Name = "Emergency Stop Ticks", GroupName = "Position Management", Order = 0)]
        public int MaxLossMargin
        {
            get { return _maxLossMargin; }
            set { _maxLossMargin = value; }
        }



        [Display(Name = "Ticks to Target", GroupName = "Position Management", Order = 0)]
        public int TicksToTarget
        {
            get { return _ticksToTarget; }
            set { _ticksToTarget = value; }
        }
        #endregion

        #region Filters

        [Display(Name = "ATR PERIOD", GroupName = "Filters", Order = 0)]
        public int atrPeriod
        {
            get { return _atrPeriod; }
            set { _atrPeriod = value; }
        }

        [Display(Name = "ATR Filter Value", GroupName = "Filters", Order = 0)]
        public double AtrFilterValue
        {
            get { return _atrFilterValue; }
            set { _atrFilterValue = value; }
        }


        [Display(Name = "Aroon PERIOD", GroupName = "Filters", Order = 0)]
        public int aroonPeriod
        {
            get { return _aroonPeriod; }
            set { _aroonPeriod = value; }
        }


        [Display(Name = "Vix Filter Value", GroupName = "Filters", Order = 0)]
        public double VixFilterValue
        {
            get { return _vixFilterValue; }
            set { _vixFilterValue = value; }
        }

        #endregion

        #region Triggers

        [Display(Name = "Stoch RSI Period", GroupName = "Triggers", Order = 0)]
        public int stochRsiPeriod
        {
            get { return _stochRsiPeriod; }
            set { _stochRsiPeriod = value; }
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

        [Display(Name = "Dynamic Stop Margin Long", GroupName = "Long", Order = 2)]
        public int LongStopMargin
        {
            get { return _longStopMargin; }
            set { _longStopMargin = value; }
        }

        [Display(Name = "ATR TARGET RATIO LONG", GroupName = "Long", Order = 3)]
        public double LongAtrRatio
        {
            get { return _longAtrRatio; }
            set { _longAtrRatio = value; }
        }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Profit Runner Long", GroupName = "Long", Order = 2)]
        public int ProfitLongRunner
        {
            get { return _profitLongRunner; }
            set { _profitLongRunner = value; }
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


        [Display(Name = "Min Ticks to Level Long", GroupName = "Long", Order = 6)]
        public int MinTicksTargetLong
        {
            get { return _minTicksTargetLong; }
            set { _minTicksTargetLong = value; }
        }

        #endregion

        #region Short
        [Display(Name = "Use Shorts", GroupName = "Short", Order = 0)]
        public bool UseShorts
        {
            get { return _useShorts; }
            set { _useShorts = value; }
        }

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Short", Order = 3)]
        public int RsiEntryShort
        {
            get { return _rsiEntryShort; }
            set { _rsiEntryShort = value; }
        }

        [Display(Name = "Dynamic Stop Margin Short", GroupName = "Short", Order = 4)]
        public int ShortStopMargin
        {
            get { return _shortStopMargin; }
            set { _shortStopMargin = value; }
        }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Profit Main Short", GroupName = "Short", Order = 1)]
        public int ProfitShortMain
        {
            get { return _profitShortMain; }
            set { _profitShortMain = value; }
        }


        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Profit Runner Short", GroupName = "Short", Order = 2)]
        public int ProfitShortRunner
        {
            get { return _profitShortRunner; }
            set { _profitShortRunner = value; }
        }

        [Display(Name = "Breakeven Trail Threshold", GroupName = "Short", Order = 5)]
        public int TrailThresholdShort
        {
            get { return _trailThresholdShort; }
            set { _trailThresholdShort = value; }
        }

        [Display(Name = "Final Trail Threshold", GroupName = "Short", Order = 6)]
        public int TrailThresholdShort2
        {
            get { return _trailThresholdShort2; }
            set { _trailThresholdShort2 = value; }
        }

        [Display(Name = "Final Trail Level Short", GroupName = "Short", Order = 7)]
        public int Trail2LevelShort
        {
            get { return _trail2LevelShort; }
            set { _trail2LevelShort = value; }
        }

        [Display(Name = "Min Ticks to Level Short", GroupName = "Short", Order = 6)]
        public int MinTicksTargetShort
        {
            get { return _minTicksTargetShort; }
            set { _minTicksTargetShort = value; }
        }

        [Display(Name = "Target ATR RATIO", GroupName = "Short", Order = 6)]
        public double ShortAtrRatio
        {
            get { return _shortAtrRatio; }
            set { _shortAtrRatio = value; }
        }

        #endregion
    }
}

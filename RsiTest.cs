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
    public class RsiTest : Strategy
    {
        #region declarations
        int _rsiPeriod = 14;
        private Indicator _rsi;
        private bool _canTrade;


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
        int rthEndTime = 220000;
        double lastWeekHigh;
        double lastWeekLow;
        double thisWeekHigh;
        double thisWeekLow;
        double IBLow;
        double IBHigh;
        int IbEndTime;
        PriceLevel[] keyLevels = { };


        #endregion

        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"RSI TEST STraaaaa!";
                Name = "RSI TEST STARATEGY";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
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

                //LEVELS Detection

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


                IsInstantiatedOnEachOptimizationIteration = true;
            }
            else if (State == State.Configure)
            {
                SetStopLoss(CalculationMode.Ticks, 100);
                //     SetProfitTarget(CalculationMode.Ticks, 200);
                AddDataSeries(BarsPeriodType.Minute, 1);
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
            if (CurrentBars[0] < BarsRequiredToTrade && CurrentBars[1] < BarsRequiredToTrade && CurrentBars[2] < 1)
                return;


            CalculateTradeTime();
            CalculateLevels();
            Print(keyLevels.Length);


            if (Position.MarketPosition != MarketPosition.Flat && ToTime(Time[0]) >= rthEndTime)
            {
                ExitLong();
                ExitShort();
            }

            if (_canTrade && BarsInProgress == 0)
            {
                if (_rsi[0] < 30 && Position.MarketPosition == MarketPosition.Flat)
                {
                    EnterLong();
                    Print("Entering Long:");
                    //	Showinfo();
                }
                else if (_rsi[0] > 80 && Position.MarketPosition == MarketPosition.Flat)
                {
                    EnterShort();
                    Print("Entering Short:");
                    //      Showinfo();
                }

                if (_rsi[0] < 30 && Position.MarketPosition == MarketPosition.Short)
                {
                    ExitShort();
                }
                else if (_rsi[0] > 80 && Position.MarketPosition == MarketPosition.Long)
                {
                    ExitLong();
                }
            }
            //Add your custom strategy logic here.
        }

        private void AddIndicators()
        {
            _rsi = RSI(BarsArray[0],rsiPeriod, 1);
            AddChartIndicator(_rsi);
        }

        private void ShowLevels(PriceLevel[] keyLevels)
        {
            foreach (PriceLevel element in keyLevels)
            {
                Print("xxxxxxxxxxxxx");
                Print(element.Name);
                Print(element.Price);
                Print("xxxxxxxxxxxxx");
            }
        }

        private void CalculateTradeTime()
        {

            if ((ToTime(Time[0]) >= 153000 && ToTime(Time[0]) < 214000))
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

            if (BarsInProgress == 2 && CurrentBars[2] >= 1) //16
            {
                lastWeekHigh = Highs[2][0];
                lastWeekLow = Lows[2][0];
                thisWeekHigh = Highs[2][0];
                thisWeekLow = Lows[2][0];
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

        private bool previousCandleRed()
        {
            return HeikenAshi8().HAOpen[0] > HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] < HeikenAshi8().HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8().HAOpen[0] < HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] > HeikenAshi8().HAClose[1];
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

        }
        

        // PARAMETERES DEFINITION

        #region Config

        [Display(Name = "RSI PERIOD", GroupName = "Config", Order = 0)]
        public int rsiPeriod
        {
            get { return _rsiPeriod; }
            set { _rsiPeriod = value; }
        }

        #endregion
    }
}

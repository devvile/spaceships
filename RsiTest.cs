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
            double[] keyLevels = {};
        #endregion

        #endregion

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"RSI TEST STraaaaa!";
				Name										= "RSI TEST STARATEGY";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 5;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0.8;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;

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


                IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
                SetStopLoss(CalculationMode.Ticks, 2-0);
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
            ShowLevels();

            if(Position.MarketPosition != MarketPosition.Flat && ToTime(Time[0]) >= rthEndTime)
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
			_rsi = RSI(rsiPeriod,1);
           AddChartIndicator(_rsi);
        }

		private void ShowLevels()
		{
			Print("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
			Print(Time[0]);
            Print("Vwapx");
            Print(VWAP(BarsArray[0]).PlotVWAP[0]);
            Print("Today Globex High:");
            Print(todayGlobexHigh);
            Print("Today Globex Low:");
            Print(todayGlobexLow);
            Print("Yesterday Globex High:");
            Print(yesterdayGlobexHigh);
            Print("Yesterday Globex Low:");
            Print(yesterdayGlobexLow);
            Print("Today RTH High:");
            Print(todayRTHHigh);
            Print("Today RTH Low:");
            Print(todayRTHLow);
            Print("Yesterday RTH High:");
            Print(yesterdayRTHHigh);
            Print("Yesterday RTH Low:");
            Print(yesterdayRTHLow);
            Print("IB High:");
            Print(IBHigh);
            Print("IB Low:");
            Print(IBLow);
            Print("This Week High:");
            Print(thisWeekHigh);
            Print("This Week Low:");
            Print(thisWeekLow);
            Print("Last Week High:");
            Print(lastWeekHigh);
            Print("Last Week Low:");
            Print(lastWeekLow);
            Print(keyLevels[13]);
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
             keyLevels = new double[]{ todayGlobexHigh, todayGlobexLow, yesterdayGlobexHigh, yesterdayGlobexLow, todayRTHHigh, todayRTHLow, yesterdayRTHHigh, yesterdayRTHLow, IBHigh, IBLow, thisWeekHigh, thisWeekLow, lastWeekHigh, lastWeekLow, VWAP(BarsArray[1]).PlotVWAP[0] };

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

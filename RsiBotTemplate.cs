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
	public class RsiBotTemplate : Strategy
	{
		#region declarations
		int _rsiPeriod = 14;
		private Indicator _rsi;
		private Indicator _levels;
		private bool _canTrade;

        #endregion

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"RSI TEST STraaaaa!";
				Name										= "RSI Template STARATEGY";
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
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
                SetStopLoss(CalculationMode.Ticks, 200);
                //     SetProfitTarget(CalculationMode.Ticks, 200);
                AddDataSeries(BarsPeriodType.Minute, 1);
           //     AddDataSeries(BarsPeriodType.Week, 1);
            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();
                AddIndicators();
            }
        }

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < BarsRequiredToTrade && CurrentBars[1] < BarsRequiredToTrade)
				return;

			if (BarsInProgress == 0) //16
			{
				if (_rsi[0] < 30 && Position.MarketPosition == MarketPosition.Flat)
				{

				}
				else if (_rsi[0] > 80 && Position.MarketPosition == MarketPosition.Flat)
				{
					EnterShort();
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

		private void Showinfo()
		{
			Print("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
			Print(Time[0]);
			/*
			Print("Globex high:");
            Print("Globex Low:");
			*/
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

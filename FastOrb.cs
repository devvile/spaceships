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
    public class FastOrb : Strategy
    {
        private int _rthStartTime;
        private int _rthEndTime;
        private int _IbEndTime;
        private int _ticksTreshold;
        private Order shortOrder;
        private Order longOrder;
        private int stopTicks = 8;
        private double rangeLow;
        private double rangeHigh;
        private bool ordersPlaced;
        private bool _canTrade;

        private List<DateRange> DateRanges { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"1 minute Scalp ORB";
                Name = "FastOrb";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
                IsInstantiatedOnEachOptimizationIteration = true;
                DateRanges = new List<DateRange>
                {
                    new DateRange(2024, 3, 11, 2024, 3, 31),
                    new DateRange(2024, 10, 27, 2024, 11, 3),
                    new DateRange(2023, 3, 12, 2023, 3, 26),
                    new DateRange(2023, 10, 29, 2023, 11, 5),
                    new DateRange(2022, 3, 13, 2022, 3, 27),
                    new DateRange(2022, 10, 30, 2022, 11, 6),
                    new DateRange(2021, 3, 14, 2022, 3, 28),
                    new DateRange(2021, 10, 31, 2022, 11, 7),
                    new DateRange(2020, 3, 8, 2022, 3, 29),
                    new DateRange(2020, 10, 25, 2022, 11, 1),
                    new DateRange(2019, 3, 8, 2022, 3, 31),
                    new DateRange(2019, 10, 27, 2022, 11, 3),
                    // Add more ranges as needed
                };
            }
            else if (State == State.Configure)
            {
                // Configuration logic
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
                return;

            CalculateTradingTime();
            CalculateTradeWindow();

            if (ToTime(Time[0]) == _rthStartTime)
            {
                rangeHigh = High[0];
                rangeLow = Low[0];
                ordersPlaced = false;
                Print(Time[0]);
                Print(Close[0]);
            }



            if (ToTime(Time[0]) > _rthStartTime && !ordersPlaced)
            {
                if (Close[0]> rangeHigh + TickThreshold * TickSize && Position.MarketPosition == MarketPosition.Flat && _canTrade && !ordersPlaced)
                {
                    longOrder = EnterLong( 1, "Long Entry");
                    double longEntryPrice = Close[0];
                }
                else if (Close[0] < rangeLow - TickThreshold * TickSize && Position.MarketPosition == MarketPosition.Flat && _canTrade && !ordersPlaced)
                {
                    double shortEntryPrice = Close[0];

                    shortOrder = EnterShort(1,"Short Entry");
                }
            }

            if (ToTime(Time[0]) >= _rthEndTime)
            {
                ExitLong();
                ExitShort();
            }
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (orderState == OrderState.Filled)
            {
                ordersPlaced = true;
                if (order.Name == "Long Entry")
                {
                    SetProfitTarget("Long Entry", CalculationMode.Ticks, StopLossTicks * 1.5);
                    SetStopLoss("Long Entry", CalculationMode.Ticks, StopLossTicks, false);

                }
                else if (order.Name == "Short Entry")
                {
                    SetProfitTarget("Short Entry", CalculationMode.Ticks, StopLossTicks * 1.5);
                    SetStopLoss("Short Entry", CalculationMode.Ticks, StopLossTicks, false);
                }
            }
        }


        private void CalculateTradeWindow()
        {

            if ((ToTime(Time[0]) >= _rthStartTime && ToTime(Time[0]) < _rthEndTime - 10000))
            {
                _canTrade = true;
            }
            else
            {
                _canTrade = false;
            }
        }

        public void CalculateTradingTime()
        {
            int intDate = ToDay(Time[0]); // Get integer representation of the date
            bool isSpecialPeriod = DateRanges.Any(range => range.Contains(intDate));
            _rthStartTime = isSpecialPeriod ? 143100 : 153100;
            _rthEndTime = isSpecialPeriod ? 210000 : 220000;
            _IbEndTime = isSpecialPeriod ? 153000 : 163000;
        }

        public class DateRange
        {
            public int StartDate { get; set; }
            public int EndDate { get; set; }

            public DateRange(int startYear, int startMonth, int startDay, int endYear, int endMonth, int endDay)
            {
                // Format the dates as strings in yyyyMMdd format and then convert to integers
                StartDate = int.Parse(string.Format("{0}{1:D2}{2:D2}", startYear, startMonth, startDay));
                EndDate = int.Parse(string.Format("{0}{1:D2}{2:D2}", endYear, endMonth, endDay));
            }

            public bool Contains(int date)
            {
                return date >= StartDate && date <= EndDate;
            }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Tick Threshold", Order = 1, GroupName = "Parameters")]
        public int TickThreshold { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Stop Loss Ticks", Order = 2, GroupName = "Parameters")]
        public int StopLossTicks { get; set; }
    }
}

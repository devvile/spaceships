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
    public class BotTemplate  : Strategy
    {
        private int _rthStartTime;
        private int _rthEndTime;
        private int _IbEndTime;
        private int _ticksTreshold;
        private Order shortOrder;
        private Order longOrder;
        private bool ordersPlaced;
        private bool _canTrade;
        double todayGlobexHigh;
        double todayGlobexLow;
        double todayIBHigh;
        double todayIBLow;
        double stopLevelHigh;
        double stopLevelLow;
        private int _numberOfRetests;
        double atrValue;
        bool blockTrade;
        Random rnd = new Random();

        private List<DateRange> DateRanges { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Bot Universal Template";
                Name = "Bot Template";
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
                    new DateRange(2021, 3, 14, 2021, 3, 28),
                    new DateRange(2021, 10, 31, 2021, 11, 7),
                    new DateRange(2020, 3, 8, 2020, 3, 29),
                    new DateRange(2020, 10, 25, 2020, 11, 1),
                    new DateRange(2019, 3, 8, 2019, 3, 31),
                    new DateRange(2019, 10, 27, 2019, 11, 3),
                    // Add more ranges as needed
                };
            }
            else if (State == State.Configure)
            {
                // Configuration logic
                AddDataSeries(BarsPeriodType.Minute, 15);
                AddDataSeries(BarsPeriodType.Day, 1);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade || CurrentBars[2] < 15)
                return;

            CalculateTradingTime();
            CalculateTradeWindow();
            UpdateLevelsValues();


            if (isStartOfSession())
            {
                refreshValuesOnStart();
            }else if(isEndOfSession())
            {
                exitActionsOnSessionEnd();
            }


            if (BarsInProgress == 0 && _canTrade)
            {
                if (LongConditions())
                {
                    //Entry
                }

                if (ShortConditions())
                {
                    //Entry
                }

            }

            if (BarsInProgress == 1)
            {
                // Timeline 2 execution rules
            }
            if (BarsInProgress == 2)
            {
                // Timeline 3 execution rules
            }



        }

        private void refreshValuesOnStart()
        {
            //refreshing Action
        }

        private void exitActionsOnSessionEnd()
        {
            //closing session actions
        }


        private bool isStartOfSession()
        {
            return ToTime(Time[0]) == _rthStartTime;
        }


        private bool isEndOfSession()
        {
            return ToTime(Time[0]) >= _rthEndTime && !noPositions();
        }

        private void CalculateTradeWindow()
        {

            if ((ToTime(Time[0]) >= _rthStartTime && ToTime(Time[0]) < _rthEndTime - 10000) && !blockTrade)
            {
                _canTrade = true;
            }
            else
            {
                _canTrade = false;
            }
        }

        private void UpdateLevelsValues()
        {
            todayGlobexHigh = LevelsFIXED().GetTodayGlobexHigh(); // Correctly call the method
            todayGlobexLow = LevelsFIXED().GetTodayGlobexLow();
            todayIBHigh = LevelsFIXED().GetTodayIBHigh(); // Correctly call the method
            todayIBLow = LevelsFIXED().GetTodayIBLow();

        }

        private bool noPositions()
        {
            return Position.MarketPosition == MarketPosition.Flat;
        }

        private bool LongConditions()
        {
            return true;
        }

        private bool ShortConditions()
        {
            return true;
        }


        public void CalculateTradingTime()
        {
            int intDate = ToDay(Time[0]); // Get integer representation of the date
            bool isSpecialPeriod = DateRanges.Any(range => range.Contains(intDate));
            _rthStartTime = isSpecialPeriod ? 144500 : 154500;
            _rthEndTime = isSpecialPeriod ? 210000 : 220000;
            _IbEndTime = isSpecialPeriod ? 144500 : 154500;
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (orderState == OrderState.Filled)
            {

            }
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {

            if (execution.Order.Name == "Long Main" && execution.Order.OrderState == OrderState.Filled && execution.Order.OrderAction == OrderAction.Buy)
            {
             //   double stopPrice = todayIBLow - ( TickSize *  StopLossTicks) ;
                SetStopLoss("Long Main", CalculationMode.Ticks, StopLossTicks, false);
                SetStopLoss("Long Runner", CalculationMode.Ticks, StopLossTicks, false);
                SetProfitTarget("Long Main", CalculationMode.Ticks, price +  2 * atrValue );
                SetProfitTarget("Long Runner", CalculationMode.Ticks, price + 4 * atrValue);
            }

            if (execution.Order.Name == "Short Main" && execution.Order.OrderState == OrderState.Filled && execution.Order.OrderAction == OrderAction.SellShort)
            {
            //    double stopPrice = todayIBHigh + (atrValue/StopLossTicks);
                SetStopLoss("Short Main", CalculationMode.Ticks, StopLossTicks, false);
                SetStopLoss("Short Runner", CalculationMode.Ticks, StopLossTicks, false);
                SetProfitTarget("Short Main", CalculationMode.Price, price - 2 * atrValue);
                SetProfitTarget("Short Runner", CalculationMode.Price, price - 4 * atrValue);
            }
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

        // Params
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Stop Loss AtR RAtio", Order = 2, GroupName = "Parameters")]
        public int StopLossTicks { get; set; }


        [Display(Name = "Number Of retests", GroupName = "Test Parameters", Order = 0)]
        public int numberOfRetests
        {
            get { return _numberOfRetests; }
            set { _numberOfRetests = value; }
        }
    }
}

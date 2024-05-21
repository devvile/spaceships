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
    public class RSSS : Strategy
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
        double todayGlobexHigh;
        double todayGlobexLow;
        double todayIBHigh;
        double todayIBLow;
        double stopLevelHigh;
        double stopLevelLow;
        private int _breakoutThreshold;
        private int _testThreshold;
        private bool breakoutLongValid;
        private bool breakoutShortValid;
        private int retestCount;
        private int _numberOfRetests;
        double atrValue;
        bool blockTrade;
        Random rnd = new Random();

        private List<DateRange> DateRanges { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"1 minute Scalp ORB";
                Name = "RSSSSSS";
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

            if (ToTime(Time[0]) == _rthStartTime)
            {
                breakoutLongValid = false;
                retestCount = 0;
            }

            if (ToTime(Time[0]) >= _rthEndTime && Position.MarketPosition != MarketPosition.Flat)
            {
                ExitLong("Exit Long After RTH", "Long Main");
                ExitShort("Exit Short After RTH", "Short Main");
                ExitLong("Exit Long After RTH", "Long Runner");
                ExitShort("Exit Short After RTH", "Short Runner");
            };

            todayGlobexHigh = Levels5().GetTodayGlobexHigh(); // Correctly call the method
            todayGlobexLow = Levels5().GetTodayGlobexLow();
            todayIBHigh = Levels5().GetTodayIBHigh(); // Correctly call the method
            todayIBLow = Levels5().GetTodayIBLow();



                if (BarsInProgress == 0 && _canTrade)
                {
                    if (Closes[0][0] > todayIBHigh  && RSI(9,1)[0] > 70  && previousCandleRed() && Aroon(BarsArray[0], 10).Up[0] > 70 && noPositions())
                    {
                        int posSize = 0;
                        if (Closes[0][0] - stopLevelLow < atrValue)
                        {
                            EnterLong(2, "Long Main");
                            EnterLong(1, "Long Runner");

                    }
                        else if (Closes[0][0] - stopLevelLow < atrValue * 2)
                        {
                            EnterLong(2, "Long Main");
                            EnterLong(1, "Long Runner");
                    }
                        else if (Closes[0][0] - stopLevelLow < atrValue * 3)
                        {
                            EnterLong(5, "Long Main");
                            EnterLong(1, "Long Runner");
                    }
                        else 
                        {
                            EnterLong(1, "Long Main");
                            EnterLong(1, "Long Runner");
                    }
                    }

                    if (Closes[0][0] < todayIBLow && RSI(9,1)[0] < 30 && previousCandleGreen() && Aroon(BarsArray[0], 10).Down[0] > 70 && noPositions())
                    {
                        EnterShort(3, "Short Main");
                        EnterShort(1, "Short Runner");
                    //    breakoutLongValid = false;
                    //  retestCount++;
                }
                    /*
                    if (Closes[0][0] < todayIBLow - atrValue && !breakoutShortValid && retestCount < numberOfRetests)
                    {
                        Print(Time[0]);
                        breakoutShortValid = true;
                        int _nr = rnd.Next();
                        string rando = Convert.ToString(_nr);
                        string name = "tag " + rando;
                        Draw.ArrowUp(this, name, true, 0, High[0] - 4 * TickSize, Brushes.Red);
                    }

                    if (breakoutShortValid) // && Lows[0][0] < todayIBHigh + (TestThreshold * TickSize))
                    {
                        EnterShort(1, "Short Main");
                        breakoutShortValid = false;
                        retestCount++;
                    }

                    /*
                    if (Lows[0][0] < todayIBHigh + (TestThreshold * TickSize) && _canTrade && noPositions() && breakoutLongValid)
                    {
                        EnterLong(1, "Long Main");
                        breakoutLongValid = false;
                    }
                    */
                }
                if(BarsInProgress == 1)
                {
                    atrValue = ATR(14)[0];
                    stopLevelHigh = Highs[1][1] + 1.5;
                    stopLevelLow = Lows[1][1] -1.5;
                }
                if (BarsInProgress == 2)
                {
                }

            

        }



        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (orderState == OrderState.Filled)
            {
                /*   ordersPlaced = true;
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
               }*/
            }
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {

            if (execution.Order.Name == "Long Main" && execution.Order.OrderState == OrderState.Filled && execution.Order.OrderAction == OrderAction.Buy)
            {
             //   double stopPrice = todayIBLow - ( TickSize *  StopLossTicks) ;
                SetStopLoss("Long Main", CalculationMode.Price, stopLevelLow, false);
                SetStopLoss("Long Runner", CalculationMode.Price, stopLevelLow, false);
                SetProfitTarget("Long Main", CalculationMode.Price, price +  2 * atrValue );
                SetProfitTarget("Long Runner", CalculationMode.Price, price + 4 * atrValue);
            }

            if (execution.Order.Name == "Short Main" && execution.Order.OrderState == OrderState.Filled && execution.Order.OrderAction == OrderAction.SellShort)
            {
            //    double stopPrice = todayIBHigh + (atrValue/StopLossTicks);
                SetStopLoss("Short Main", CalculationMode.Price, stopLevelHigh, false);
                SetStopLoss("Short Runner", CalculationMode.Price, stopLevelHigh, false);
                SetProfitTarget("Short Main", CalculationMode.Price, price - 2 * atrValue);
                SetProfitTarget("Short Runner", CalculationMode.Price, price - 4 * atrValue);
            }
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
        private bool previousCandleRed()
        {
            return HeikenAshi8(BarsArray[0]).HAOpen[0] > HeikenAshi8(BarsArray[0]).HAClose[0] && HeikenAshi8(BarsArray[0]).HAOpen[1] < HeikenAshi8(BarsArray[0]).HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8(BarsArray[0]).HAOpen[0] < HeikenAshi8(BarsArray[0]).HAClose[0] && HeikenAshi8(BarsArray[0]).HAOpen[1] > HeikenAshi8(BarsArray[0]).HAClose[1];
        }


        private bool noPositions()
        {
            return Position.MarketPosition == MarketPosition.Flat;
        }


        public void CalculateTradingTime()
        {
            int intDate = ToDay(Time[0]); // Get integer representation of the date
            bool isSpecialPeriod = DateRanges.Any(range => range.Contains(intDate));
            _rthStartTime = isSpecialPeriod ? 144500 : 154500;
            _rthEndTime = isSpecialPeriod ? 210000 : 220000;
            _IbEndTime = isSpecialPeriod ? 144500 : 154500;
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

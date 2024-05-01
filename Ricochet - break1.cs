using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.IO;
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
    public class RicochetBreak1 : Strategy
    {
        #region declarations


        private Order _longOneOrder;
        private Order _longTwoOrder;
        private int _lotSize1;
        private int _lotSize2;
        private int _target1;
        private int _target2;
        private int _stop;
        private StreamWriter sw;
        private bool writeHeaders;
        private ISet<int> tradesTracking;
        private Indicator _levels;
        private int _numberOfRetests;
        private int _testTreshold;
        private int _breakoutTreshold;
        private int retestCount;
        private DateTime lastResetTime;
        private bool _takeTests;
        private bool _breakoutValid;
        int _rthStartTime;
        int _rthEndTime;
        int _IbEndTime;
        double rangeHigh;
        double rangeLow;
        public string CsvFilePath = @"C:\Users\ZALMAN Z3 ICEBERG\Documents\backtesting\backtesting.csv";

        private double _longStopMargin;
        private double _shortStopMargin;

        #endregion

        #region My Parameters

        private int _entryFastStochValueLong;
        private int _baseStopMarginLong;
        private double _targetRatioLong;
        private double atrValue;
        private double stopLossPrice;
        private double _longEntryPrice1;
        private bool _canTrade = false;

        private int BarNr = 0;  //sprawdzic <-----
        Random rnd = new Random();

        private string status = "Flat";


        #endregion


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Retest Tracker";
                Name = "Ricochet Break1";
                Calculate = Calculate.OnBarClose;
                BarsRequiredToTrade = 60;
                lastResetTime = DateTime.MinValue;
                _numberOfRetests =1;
                _breakoutValid = false;


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

                _rthStartTime = 153000;
                _rthEndTime = 220000;
                _IbEndTime = 163000;
                //   
            }


            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;

                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                Levels4 myLevels4 = Levels4();
                AddChartIndicator(myLevels4);
            //    AddDataSeries(BarsPeriodType.Minute, 15);
                AddDataSeries(BarsPeriodType.Day, 1);
            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();
                Calculate = Calculate.OnBarClose;
            }
            else if (State == State.Terminated)
            {
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                    sw = null;
                }
            }
        }

        private List<DateRange> DateRanges { get; set; }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade)
                return;

            int intDate = ToDay(Time[0]); // Get integer representation of the date
            bool isSpecialPeriod = DateRanges.Any(range => range.Contains(intDate));
            _rthStartTime = isSpecialPeriod ? 143000 : 153000;
            _rthEndTime = isSpecialPeriod ? 210000 : 220000;
            _IbEndTime = isSpecialPeriod ? 153000 : 163000;

            if (ToTime(Time[0]) == _rthStartTime)// && (lastResetTime.Date != Time[0].Date))
            {
                retestCount = 0;
                lastResetTime = Time[0];
                rangeHigh = Levels4().GetTodayGlobexHigh(); // Initially set rangeHigh to today's Globex high at the start of the session
                  rangeLow = Levels4().GetTodayGlobexLow();
            }

            CalculateTradeTime();
            if (Position.MarketPosition != MarketPosition.Flat && ToTime(Time[0]) >= _rthEndTime)
            {

                ExitLong();
            }

            if (retestCount >= numberOfRetests)
                return;

            if (!_takeTests)
            {
                return;
            }



            if (_canTrade)
            {

                if (BarsInProgress == 0)
                { /*
                    double todayGlobexHigh = Levels4().GetTodayGlobexHigh(); // Correctly call the method
                    double todayGlobexLow = Levels4().GetTodayGlobexLow();
                    double todayIBHigh = Levels4().GetTodayIBHigh(); // Correctly call the method
                    double todayIBLow = Levels4().GetTodayIBLow();
                    */
                    if  (Highs[0][4] > rangeHigh && noPositions())
                    {
                        rangeHigh = Highs[0][4];
                    }/*
                    else
                    {
                        rangeHigh = todayGlobexHigh;
                    }
                    */
                    /*      if (todayGlobexLow < todayIBLow)
                          {
                              rangeLow = todayGlobexLow;
                          }
                          else
                          {
                              rangeLow = todayIBLow;
                          }
                    */
                    Print("rangeHigh");
                    Print(rangeHigh);
                    //make trade Active
                    //check if there was stop without tp1
                    //check if there was t1 with 8p without stop
                    //check if there was t1 with 10p without stop
                    //check if there was t2 with 20p without stop
                    //check if maximum tp2 range  without stop
                    // na koneic dnia tworzony jest array z kazdego valid entry

                    //check if breakout was valid
                    if (Closes[0][0]>= rangeHigh + (TickSize * breakoutTreshold) && !_breakoutValid)// && noPositions()) // && Closes[0][0] > emaValue)
                    {
                        int _nr = rnd.Next();
                        string rando = Convert.ToString(_nr);
                        string name = "tag " + rando;
                        _breakoutValid = true;
                        Draw.ArrowUp(this, name, true, 0, Low[0] - 4 * TickSize, Brushes.Blue);
                        _longOneOrder = EnterLong(LotSize1, "Long Base");
                        _longTwoOrder = EnterLong(LotSize2, "Long Runner");
                    }
                    /*
                    if (Closes[0][0] <= rangeLow - (TickSize * breakoutTreshold) && !_breakoutValid && noPositions()) // &&  rvol> rvolTreshold)
                    {
                        int _nr = rnd.Next();
                        string rando = Convert.ToString(_nr);
                        string name = "tag " + rando;
                        _breakoutValid = true;
                        Draw.ArrowDown(this, name, true, 0, High[0] + 4 * TickSize, Brushes.Red);

                    }
                    */

                    if (_breakoutValid)
                    {
                        retestCount++;
              //          SaveToCsv();
                        _breakoutValid = false;
                    }

                }

                else if (BarsInProgress == 1) 
                {

                }

            }


        }



        private void CalculateTradeTime()
        {

            if ((ToTime(Time[0]) >= _rthStartTime && ToTime(Time[0]) < _rthEndTime))
            {
                _canTrade = true;
                _takeTests = true;
            }
            else
            {
                _canTrade = false;
                _takeTests = false;
            }
        }



        private bool noPositions()
        {
            return Position.MarketPosition == MarketPosition.Flat;
        }

        private void SaveToCsv()
        {
            if (!File.Exists(CsvFilePath))
            {
                Print("Creating file:");
                Print(CsvFilePath); 
                sw = File.AppendText(CsvFilePath);
                sw.WriteLine("Date,RangeHigh,DayHigh");
                string currentDate = Time[0].ToString("yyyy-MM-dd");
                double dayHigh = Highs[1][0]; // Assuming Highs[1] is your day candle data series
                sw.WriteLine(String.Format("{0},{1},{2}", currentDate, rangeHigh, dayHigh));
                sw.Close();

            }
            else
            {
                sw = File.AppendText(CsvFilePath);
                string currentDate = Time[0].ToString("yyyy-MM-dd");
                double dayHigh = Highs[1][0]; // Assuming Highs[1] is your day candle data series
                sw.WriteLine(String.Format("{0},{1},{2}", currentDate, rangeHigh, dayHigh));
                sw.Close();
            }

            // Append new data to the existing file
            /*
            using (StreamWriter sw = File.AppendText(CsvFilePath))
            {
       
            }*/

        }

        private bool OrderFilled(Order order)
        {
            return order.OrderState == OrderState.Filled;
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
OrderState orderState, DateTime time, ErrorCode error, string comment)
        {


            if (OrderFilled(order))
            {
                    SetStopLoss(CalculationMode.Ticks, Stop);


                if (order == _longOneOrder)
                {
                    SetProfitTarget("Long Base", CalculationMode.Ticks,Target1);
                }
                else if (order == _longTwoOrder)
                {
                    //runner
                    SetProfitTarget("Long Runner", CalculationMode.Ticks,  Target2);
                }

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


        #region Properties

        [Display(Name = "Target Base (Ticks)", GroupName = "Position Management", Order = 0)]
        public int Target1
        {
            get { return _target1; }
            set { _target1 = value; }
        }

        [Display(Name = "Target Runner (Ticks)", GroupName = "Position Management", Order = 0)]
        public int Target2
        {
            get { return _target2; }
            set { _target2 = value; }
        }

        [Display(Name = "Stop", GroupName = "Position Management", Order = 0)]
        public int Stop
        {
            get { return _stop; }
            set { _stop = value; }
        }

        [Display(Name = "Size base", GroupName = "Position Management", Order = 0)]
        public int LotSize1
        {
            get { return _lotSize1; }
            set { _lotSize1 = value; }
        }
        [Display(Name = "Size Runner", GroupName = "Position Management", Order = 0)]
        public int LotSize2
        {
            get { return _lotSize2; }
            set { _lotSize2 = value; }
        }


        #region  Test Parameters

        [Display(Name = "Number Of retests", GroupName = "Test Parameters", Order = 0)]
        public int numberOfRetests
        {
            get { return _numberOfRetests; }
            set { _numberOfRetests = value; }
        }

        [Display(Name = "Breakout Treshold (ticks)", GroupName = "Test Parameters", Order = 1)]
        public int breakoutTreshold
        {
            get { return _breakoutTreshold; }
            set { _breakoutTreshold = value; }
        }

        [Display(Name = "Test Treshold (ticks)", GroupName = "Test Parameters", Order = 1)]
        public int testTreshold
        {
            get { return _testTreshold; }
            set { _testTreshold = value; }
        }


        #endregion



        #endregion

    }


}
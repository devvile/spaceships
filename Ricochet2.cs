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
    public class Ricochet2 : Strategy
    {
        #region declarations

        private Indicator _aroon;
        private Indicator _levels;
        private Indicator _aroonFast;
        private Indicator _rvol;
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
        public int _stop;
        public int _target1;
        public int _target2;

        private Order _longOneOrder;
        private Order _longTwoOrder;
        private Order _shortOneOrder;
        private Indicator _atr;
        private Indicator _ema;
        private Indicator _stoch;
        private Indicator _aroonSlow;
        private Indicator _aroonMid;


        private double _aroonUp;
        private int _barsToCheck;
        private double _longStopMargin;
        private double _shortStopMargin;
        private double _atrTargetRatio;
        private double shortPrice;
        private int _atrPeriod = 14;
        private double rvolTreshold;
        #endregion

        #region My Parameters

        private int _entryFastStochValueLong;
        private int _baseStopMarginLong;
        private double _targetRatioLong;

        private int _entryFastStochValueShort;
        private int _baseStopMarginShort;
        private double _targetRatioShort;

        private int _aroonPeriod;

        private int _aroonFilter;
        private int _aroonPeriodFast;
        private int _aroonPeriodSlow;
        private double atrValue;
        private double emaValue;
        private double stopLossPrice;
        private double _shortEntryPrice1;
        private double _longEntryPrice1;

        private int _lotSize1;
        private int _lotSize2;


        private bool _useLongs = true;
        private bool _useShorts = true;
        private bool _useAroon = true;
        private bool _canTrade = false;
        private bool UseRsi = true;


        private int BarNr = 0;  //sprawdzic <-----
        Random rnd = new Random();

        private string status = "Flat";


        #endregion


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Retest Tracker";
                Name = "Ricochet2";
                Calculate = Calculate.OnBarClose;
                BarsRequiredToTrade = 60;
                lastResetTime = DateTime.MinValue;
                _numberOfRetests =1;
                _breakoutValid = false;
                /*
                rvolTreshold = 1.5;
                _stop = 30;
                _target1 = 60;
                _target2 = 90;
                */

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
                Levels5 myLevels5 = Levels5();
                AddChartIndicator(myLevels5);

                AddDataSeries(BarsPeriodType.Minute, 15);
            }
            else if (State == State.DataLoaded)
            {

                ClearOutputWindow();
                AddIndicators();
                Calculate = Calculate.OnBarClose;
            }
        }

        private List<DateRange> DateRanges { get; set; }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade)//   CurrentBars[2] < BarsRequiredToTrade)
                return;


            int intDate = ToDay(Time[0]); // Get integer representation of the date
            bool isSpecialPeriod = DateRanges.Any(range => range.Contains(intDate));
            _rthStartTime = isSpecialPeriod ? 153000 : 163000;
            _rthEndTime = isSpecialPeriod ? 210000 : 220000;
            _IbEndTime = isSpecialPeriod ? 153000 : 163000;
            CalculateTradeTime();
            if (ToTime(Time[0]) == _rthStartTime) //&& (lastResetTime.Date != Time[0].Date))
            {
                retestCount = 0;
                lastResetTime = Time[0];
                rangeHigh = Levels5().GetTodayGlobexHigh(); // Initially set rangeHigh to today's Globex high at the start of the session
                rangeLow = Levels5().GetTodayGlobexLow();
            }



            if (retestCount >= numberOfRetests)
                return;

            if (!_takeTests)
            {
                return;
            }

                if (Position.MarketPosition == MarketPosition.Flat)
            {
                status = "Flat";

            }
            if (Position.MarketPosition != MarketPosition.Flat)
            {
                double entryPrice = Position.AveragePrice;
                double currentPrice = Closes[0][0];

                // Check if the current close is 10 points above the entry price

                if (currentPrice >= entryPrice + 5)
                {
                    // Update stop loss to the entry price
                    SetStopLoss(CalculationMode.Price, rangeHigh - 1);
                }
                if (currentPrice >= entryPrice + 10)
                {
                    // Update stop loss to the entry price
                    SetStopLoss(CalculationMode.Price, entryPrice);
                }
                if (currentPrice >= entryPrice + atrValue * Target1)
                {
                    // Update stop loss to the entry price
                    SetStopLoss(CalculationMode.Price, entryPrice + 4);
                }
            };


            if (_canTrade)
            {

                if (BarsInProgress == 0)
                {
                    /*
                    double todayGlobexHigh = Levels4().GetTodayGlobexHigh(); // Correctly call the method
                    double todayGlobexLow = Levels4().GetTodayGlobexLow();
                    double todayIBHigh = Levels4().GetTodayIBHigh(); // Correctly call the method
                    double todayIBLow = Levels4().GetTodayIBLow();
                 //   double rvol = ReVOLT(BarsArray[0], 10, 1.35, 0.8).GetRevol();
                    */
                    //   Print("Rvol eee");
                    //   Print(Time[0]);
                    //     Print(rvol);

                    if (Position.MarketPosition != MarketPosition.Flat && ToTime(Time[0]) >= _rthEndTime)
                    { 
                        ExitLong();
                    }

                    if (Highs[0][4] > rangeHigh && noPositions())
                    {
                        rangeHigh = Highs[0][4];
                    }
                    if (Lows[0][4] < rangeLow && noPositions())
                    {
                        rangeLow = Lows[0][4];
                    }

                    /*
                    if (todayGlobexHigh > todayIBHigh)
                    {
                        rangeHigh = todayGlobexHigh;
                    }
                    else
                    {
                        rangeHigh = todayIBHigh;
                    }

                    if (todayGlobexLow < todayIBLow)
                    {
                        rangeLow = todayGlobexLow;
                    }
                    else
                    {
                        rangeLow = todayIBLow;
                    }*/

                    //check if breakout was valid
                    if (Closes[0][0]>= rangeHigh + (TickSize * breakoutTreshold) && !_breakoutValid && noPositions())// && Closes[0][0] > emaValue)
                    {
                        int _nr = rnd.Next();
                        string rando = Convert.ToString(_nr);
                        string name = "tag " + rando;
                        _breakoutValid = true;
                        Draw.ArrowUp(this, name, true, 0, Low[0] - 4 * TickSize, Brushes.Blue);
                        _longOneOrder = EnterLong(LotSize1, "Long Base");
                        _longTwoOrder = EnterLong(LotSize2, "Long Runner");

                        //    EnterLongLimit(0, true, 3, longPrice, "Long");
                    }

                    if (Closes[0][0] <= rangeLow - (TickSize * breakoutTreshold) && !_breakoutValid && noPositions()) // &&  rvol> rvolTreshold)
                    {
                        int _nr = rnd.Next();
                        string rando = Convert.ToString(_nr);
                        string name = "tag " + rando;
                        _breakoutValid = true;
                        Draw.ArrowDown(this, name, true, 0, High[0] + 4 * TickSize, Brushes.Red);

               //         shortPrice = rangeLow - (testTreshold * TickSize);
               //         EnterShortLimit(0, true, 3, shortPrice, "Short");
                    }
                    /*
                    if (Close[0] - Position.AveragePrice > 10)
                    {
                      //  SetStopLoss("Long Runner",CalculationMode.Price, Position.AveragePrice, false);
                      //  SetStopLoss("Long Base", CalculationMode.Price, Position.AveragePrice, false);
                    }*/

                    if (_breakoutValid)
                    {
                        retestCount++;
                        _breakoutValid = false;
                    }


                    //cena high musi sie znalezc powyzej poziomu glboec high o breakTreshold value
                    // cena potem musi opasc do poziomu minimum glboxHigh plus close trehsold
                    // rozpoczecie trackowania
                    // 
                }

                else if (BarsInProgress == 1) // 15 min
                {
                    atrValue = _atr[0];
                    emaValue = _ema[0];
                    // Exits


                }
                /*
                else if (BarsInProgress == 2) // 1 min
                {


                }*/
            }


        }


        // zaimplementowac czasy Ziomy i letni dla usa dla RTH i dla LEVELS


        private void CalculateTradeTime()
        {

            if ((ToTime(Time[0]) >= _rthStartTime && ToTime(Time[0]) < _rthEndTime - 15))
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

        private void AdjustStop()
        {

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
                if (averageFillPrice - rangeHigh > 7)
          //      if (averageFillPrice - rangeHigh > atrValue * 1.5 )
                {
                    SetStopLoss(CalculationMode.Ticks, Stop * 2 * TickSize);
                }
                else
                {
                    SetStopLoss(CalculationMode.Price, rangeHigh - (Stop * TickSize));
                }

                if (order == _longOneOrder)
                {
                    SetProfitTarget("Long Base",CalculationMode.Price, averageFillPrice + atrValue * Target1);
                }else if (order == _longTwoOrder)
                {
                    //runner
                    SetProfitTarget("Long Runner", CalculationMode.Price, averageFillPrice + atrValue * Target2);
                }

            }
        }

        private void AddIndicators()
        {
            _atr = ATR(BarsArray[1], AtrPeriod);
            _ema = ATR(BarsArray[1], 14);
            //     _aroonSlow = Aroon(BarsArray[0], AroonPeriodSlow);
            //  AddChartIndicator(_atr);
            //     AddChartIndicator(_aroonSlow);
            _rvol = ReVOLT(BarsArray[0], 10, 1.3, 0.8);
            AddChartIndicator(_rvol);
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


        #region Params


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

        #region Position Management




        [Display(Name = "Atr Target ratio", GroupName = "Position Management", Order = 0)]
        public double AtrTargetRatio
        {
            get { return _atrTargetRatio; }
            set { _atrTargetRatio = value; }
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


        [Display(Name = "Target Base (atr ratio)", GroupName = "Position Management", Order = 0)]
        public int Target1
        {
            get { return _target1; }
            set { _target1 = value; }
        }

        [Display(Name = "Target Runner (atr ratio)", GroupName = "Position Management", Order = 0)]
        public int Target2
        {
            get { return _target2; }
            set { _target2 = value; }
        }



        [Display(Name = "Stop (Ticks)", GroupName = "Position Management", Order = 0)]
        public int Stop
        {
            get { return _stop; }
            set { _stop = value; }
        }

        #endregion

        #region Filters

        [Display(Name = "Atr period", GroupName = "Filters", Order = 0)]
        public int AtrPeriod
        {
            get { return _atrPeriod; }
            set { _atrPeriod = value; }
        }


        #endregion


        #region Aroon

        [Display(Name = "Aroon Period Fast", GroupName = "Aroon", Order = 0)]
        public int AroonPeriodFast
        {
            get { return _aroonPeriodFast; }
            set { _aroonPeriodFast = value; }
        }


        [Display(Name = "Aroon Period Slow", GroupName = "Aroon", Order = 0)]
        public int AroonPeriodSlow
        {
            get { return _aroonPeriodSlow; }
            set { _aroonPeriodSlow = value; }
        }

        [Display(Name = "Aroon Entry Filter", GroupName = "Aroon", Order = 0)]
        public int AroonFilter
        {
            get { return _aroonFilter; }
            set { _aroonFilter = value; }
        }




        #endregion

        #endregion

    }


}
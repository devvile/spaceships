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
    public class Ricochet : Strategy
    {
        #region declarations

        private Indicator _aroon;
        private Indicator _levels;
        private Indicator _aroonFast;
        private Indicator _rvol;
        private Indicator _kama;
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
        private Indicator _stoch;
        private Indicator _aroonSlow;
        private Indicator _aroonMid;


        double todayGlobexHigh;
        double todayGlobexLow;
        double todayIBHigh;
        double todayIBLow;
        private double _aroonUp;
        private double _longStopMargin;
        private double _shortStopMargin;
        private double _atrTargetRatio;
        private double shortPrice;
        private int _atrPeriod = 14;
        private int _maxStop = 500;
        private double rvolTreshold;
        #endregion

        #region My Parameters

        private int _entryFastStochValueLong;
        private int _baseStopMarginLong;
        private double _targetRatioLong;


        private int _aroonPeriod;

        private int _aroonFilter;
        private double atrValue;
        private double stopLossPrice;
        private double _shortEntryPrice1;
        private double _longEntryPrice1;

        private int _lotSize1;
        private int _lotSize2;


        private bool _useLongs = true;
        private bool _useShorts = true;
        private bool _canTrade = false;


        private int BarNr = 0;  //sprawdzic <-----
        Random rnd = new Random();

        private string status;


        #endregion


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Retest Tracker";
                Name = "Ricochet";
                Calculate = Calculate.OnBarClose;
                BarsRequiredToTrade = 60;
                lastResetTime = DateTime.MinValue;
                _numberOfRetests = 1;
                _breakoutValid = false;
                rvolTreshold = 1.5;
                _stop = 12;
                _target1 = 4;
                _target2 = 10;


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
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;

                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                Levels4 myLevels4 = Levels4();
                AddChartIndicator(myLevels4);

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
            if (CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade)
                return;


            int intDate = ToDay(Time[0]); // Get integer representation of the date
            bool isSpecialPeriod = DateRanges.Any(range => range.Contains(intDate));
            _rthStartTime = isSpecialPeriod ? 143000 : 153000;
            _rthEndTime = isSpecialPeriod ? 210000 : 220000;
            _IbEndTime = isSpecialPeriod ? 153000 : 163000;

            if (ToTime(Time[0]) == _rthStartTime && (lastResetTime.Date != Time[0].Date))
            {
                retestCount = 0;
                lastResetTime = Time[0];
            }

            CalculateTradeTime();


            if (ToTime(Time[0]) >= _rthEndTime && Position.MarketPosition == MarketPosition.Long)
            {
                ExitLong("Exit Long After RTH", "Long Base");
                ExitLong("Exit Long After RTH", "Long Runner");
       //         ExitLong("Exit Long After RTH", "Addon");
            };


            if (!_takeTests)
            {
                return;
            }


    

            if (_canTrade)
            {

                if (BarsInProgress == 0)
                {

                    todayGlobexHigh = Levels4().GetTodayGlobexHigh(); // Correctly call the method
                    todayGlobexLow = Levels4().GetTodayGlobexLow();
                    todayIBHigh = Levels4().GetTodayIBHigh(); // Correctly call the method
                    todayIBLow = Levels4().GetTodayIBLow();


                    if (todayGlobexHigh > todayIBHigh && noPositions())
                    {
                        rangeHigh = todayGlobexHigh;
                    }
                    else
                    {
                        rangeHigh = todayIBHigh;
                    }

                    if (todayGlobexLow < todayIBLow && noPositions())
                    {
                        rangeLow = todayGlobexLow;
                    }
                    else
                    {
                        rangeLow = todayIBLow;
                    }


                    //check if breakout was valid
                    if (Closes[0][0] >= rangeHigh + (TickSize * breakoutTreshold) && !_breakoutValid && noPositions() && rangeHigh-todayGlobexHigh < 25 && retestCount < numberOfRetests) 
                    {
                        int posSize = 0;
                        /*
                        if(rangeHigh - todayGlobexHigh < atrValue * 2)
                        {
                            posSize = LotSize2;
                        }
                        else if (rangeHigh - todayGlobexHigh <= atrValue * 4)
                        {
                            posSize = LotSize2/2;
                        }
                        else
                        {
                            posSize = (LotSize2 / 3);
                        }*/
                        double stopSize = CalculateStopLoss();
                        Print(Time[0]);
                        Print(CalculateStopLoss());

                        if (rangeHigh - todayGlobexHigh <= atrValue)
                        {
                            posSize = LotSize2 + 1;
                        }
                       else if (rangeHigh - todayGlobexHigh <= atrValue * 2)
                        {
                            posSize = LotSize2;
                        }
                        else if (rangeHigh - todayGlobexHigh <= atrValue * 4)
                        {
                            posSize = LotSize2 / 2;
                        }
                        else
                        {
                            posSize = (LotSize2 / 3);
                        }
                //       if ((posSize + LotSize1) * stopSize * 5 < MaxStop)
                        {
                            int _nr = rnd.Next();
                            string rando = Convert.ToString(_nr);
                            string name = "tag " + rando;
                            _breakoutValid = true;
                            Draw.ArrowUp(this, name, true, 0, Low[0] - 4 * TickSize, Brushes.Blue);
                             _longOneOrder = EnterLong(LotSize1, "Long Base");
                            _longTwoOrder = EnterLong(posSize, "Long Runner");
                        }

                    }

                    if (Closes[0][0] <= rangeLow - (TickSize * breakoutTreshold) && !_breakoutValid && noPositions()) 
                    {
                        int _nr = rnd.Next();
                        string rando = Convert.ToString(_nr);
                        string name = "tag " + rando;
                        _breakoutValid = true;
                        Draw.ArrowDown(this, name, true, 0, High[0] + 4 * TickSize, Brushes.Red);
                    }

                    if (noPositions())
                    {
                        SetStopLoss("Long Base", CalculationMode.Ticks, 40, false);
                        SetStopLoss("Long Runner", CalculationMode.Ticks, 40, false);
                        status = "Flat";
                    }
                    else
                    {
                        Trail();

                        if (status == "Breakeven" && !noPositions())
                        {
                 //          EnterLong(2, "Addon");
                        }

                        AdjustStop();   
                    };


                    if (_breakoutValid)
                    {
                        retestCount++;
                        _breakoutValid = false;
                    }

                }

                else if (BarsInProgress == 1) // 4 min
                {
                    atrValue = _atr[0];
                }
            }


        }

        private void Trail()
        {
            double entryPrice = _longEntryPrice1;
            double currentPrice = Close[0];


                if (Close[0] >= entryPrice + (atrValue * 0.75) && status != "Breakeven" && status != "Trail2" && status != "Level" && !(entryPrice - rangeHigh > 8))
                {
                    status = "Level";
                }
                if (Close[0] >= entryPrice + atrValue * 2  && status != "Breakeven" && status !="Trail2")
                {
                    status = "Breakeven";
                }
                if (High[0] >= entryPrice + atrValue * Target1  && status == "Breakeven" )
                {

                    status = "Trail2";
                }

        }

        private void AdjustStop()
        {
                double entryPrice = _longEntryPrice1;

            if (status == "Level")
            {
                SetStopLoss("Long Runner",CalculationMode.Price, rangeHigh-1,false);
                SetStopLoss("Long Base",CalculationMode.Price, rangeHigh-1,false);

            } 
             if (status == "Breakeven")
            {
                SetStopLoss("Long Runner", CalculationMode.Price, entryPrice, false);
                SetStopLoss("Long Base", CalculationMode.Price, entryPrice, false);
  //              SetStopLoss("Addon", CalculationMode.Price, entryPrice, false);
            }
                if (status == "Trail2")
            {
               SetStopLoss("Long Runner", CalculationMode.Price, Bollinger(2, 10).Lower[0] - atrValue/2, false);
               SetStopLoss("Long Base", CalculationMode.Price, Bollinger(2, 10).Lower[0] - atrValue/2, false);
//               SetStopLoss("Addon", CalculationMode.Price, Bollinger(2, 10).Lower[0] - atrValue / 2, false);

            }

               
        }

        private void CalculateTradeTime()
        {

            if ((ToTime(Time[0]) >= _rthStartTime && ToTime(Time[0]) < _rthEndTime - 10000))
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



        private bool OrderFilled(Order order)
        {
            return order.OrderState == OrderState.Filled;
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {


            if (OrderFilled(execution.Order)) //moze to jest problemem
            {
                if(execution.Order.Name == "Profit target")
                {
                 //   SetStopLoss("Long Runner", CalculationMode.Price, KAMA(BarsArray[0], 10, 14, 30)[1], false);
                };
                if (price - rangeHigh > (32*TickSize))
                {
                    SetStopLoss("Long Base", CalculationMode.Ticks, Stop  * 1.5 , false);
                    SetStopLoss("Long Runner",CalculationMode.Ticks, Stop *1.5 ,false);
                }
                else
                {
                    SetStopLoss("Long Base", CalculationMode.Price, todayGlobexHigh - (Stop * TickSize),false);
                    SetStopLoss("Long Runner", CalculationMode.Price, todayGlobexHigh - (Stop * TickSize),false);
                }

                if (execution.Order == _longOneOrder)
                {
                    _longEntryPrice1 = price;
               //     SetProfitTarget("Long Base", CalculationMode.Price, execution.Order.AverageFillPrice + atrValue * Target1);
             //         ExitLongLimit(0,true,LotSize1, execution.Order.AverageFillPrice + atrValue * Target1, "Profit Target1", "Long Base");
                    status = "Long Default";
                }
                else if (execution.Order == _longTwoOrder)
                {
                    _longEntryPrice1 = price;
                    status = "Long Default";
                    //           ExitLongLimit(LotSize2, execution.Order.AverageFillPrice + atrValue * Target2, "Profit Target2", "Long Runner");
                }

            }
        }

        private double CalculateStopLoss()
        {
            double stopLoss = 0;
            if (Close[0] - rangeHigh > 32 * TickSize){
                stopLoss = Stop * 2 * TickSize;
            }
            else
            {
                stopLoss = Close[0] - (todayGlobexHigh - Stop * TickSize);
            }
            return stopLoss;
        }

        private void AddIndicators()
        {
            _atr = ATR(BarsArray[1], AtrPeriod);
            _kama = KAMA(BarsArray[0], 10,14,30);
            _rvol = ReVOLT(BarsArray[0], 10, 1.3, 0.8);
            AddChartIndicator(_rvol);
            AddChartIndicator(_kama);
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

        [Display(Name = "Max Stop Loss ($)", GroupName = "Filters", Order = 0)]
        public int MaxStop
        {
            get { return _maxStop; }
            set { _maxStop = value; }
        }


        #endregion



        #endregion

    }


}
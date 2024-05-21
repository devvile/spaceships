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
    public class SOJUZ : Strategy
    {
        #region declarations

        private Indicator _levels;
        private Indicator _rvol;
        private Indicator _kama;
        private Indicator _atr;
        private Indicator _atrDay;
        private int _numberOfRetests;
        private int _testTreshold;
        private int _breakoutTreshold;
        private int retestCount;
        private double atrDayValue;
        private DateTime lastResetTime;
        private bool _takeTests;
        private bool _breakoutValid;
        int _rthStartTime;
        int _rthEndTime;
        int _IbEndTime;
        double rangeHigh;
        double rangeLow;
        public int _StopLevel; //from globex
        public int _StopPosition; //from position
        public int _target1;
        public int _target2;
        private bool _isLongMainStopped; 

        private Order _longOneOrder;


        private bool _addSize;

        private List<Order> addonOrders; 

        double todayGlobexHigh;
        double todayGlobexLow;
        double todayIBHigh;
        double todayIBLow;

        private double _aroonUp;
        private double _atrTargetRatio;
        private int _atrPeriod = 14;
        private int _maxStop = 500;
        private double rvolTreshold;
        private int _amountForSizeUp;

        private Account account;
        #endregion

        #region My Parameters

        private int _baseStopMarginLong;
        private double _targetRatioLong;


        private int _aroonPeriod;

        private int _aroonFilter;
        private double atrValue;
        private double stopLossPrice;
        private double _shortEntryPrice1;
        private double _longEntryPrice1;

        private int _lotSize1;
        private int LotSize1;

        private bool _useAddon;

        private bool _useLongs = true;
        private bool _useShorts = true;
        private bool _canTrade = false;
        private int _initialLotSize;
        private int initialLotSize;
        private int _maxLotSize;


        private int BarNr = 0;  //sprawdzic <-----
        Random rnd = new Random();

        private string status;

        #endregion
        // ADDONY NIE LAPIA STOPA

        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Retest Tracker";
                Name = "SOJUZ Resurrected";
                Calculate = Calculate.OnBarClose;
                BarsRequiredToTrade = 60;
                lastResetTime = DateTime.MinValue;
                _numberOfRetests = 1;
                _breakoutValid = false;
                rvolTreshold = 1.5;
                _StopLevel = 12;
                _target1 = 4;
                _target2 = 10;
                _maxLotSize = 30;

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
                Calculate = Calculate.OnBarClose;

                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                Levels4 myLevels4 = Levels4();
                AddChartIndicator(myLevels4);
                _isLongMainStopped = false;
                addonOrders = new List<Order>();
                AddDataSeries(BarsPeriodType.Minute, 15);
                AddDataSeries(BarsPeriodType.Day, 1);
            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();
                account = Account;
                AddIndicators();
                Calculate = Calculate.OnBarClose;
            }
        }

        private List<DateRange> DateRanges { get; set; }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade || CurrentBars[2] <= 14)
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
                ExitLong("Exit Long After RTH", "addon");
                ExitLong("Exit Long After RTH", "Long Main");
            };


            if (!_takeTests)
            {
                return;
            }


    

            if (_canTrade)
            {

                if (BarsInProgress == 0)
                {
                    LotSize1 = UserLotSize;
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

                    if (noPositions())
                    {
                        SetStopLoss("Long Main", CalculationMode.Ticks, 40, false);
                        status = "Flat";
                    }
                    else
                    {


                        Trail();
                                                if (status == "Trail2" && !noPositions() && previousCandleRed() && Aroon(10).Up[0] > 70 && Aroon(10).Down[0] < 35 &&  !_isLongMainStopped && useAddon)// && StochRSI(14)[0]<=0.2)
                        {
                            EnterLong(1, "addon");
                        }
                        AdjustStop();
                    };
                    if (addSize)
                    {
                        UpdateLotSizeBasedOnProfit();
                    };
                    //check if breakout was valid
                    if (Closes[0][0] >= rangeHigh + (TickSize * breakoutTreshold) && !_breakoutValid && noPositions()  && retestCount < numberOfRetests &&  rangeHigh-todayGlobexHigh < 25)//  && previousCandleRed())
                    {
                        int posSize = 0;
                        double stopSize = CalculateStopLoss();
      

                        if (rangeHigh - todayGlobexHigh <= atrValue)
                        {
                            posSize = (int)(LotSize1 * 1.5);
                        }
                       else if (rangeHigh - todayGlobexHigh <= atrValue * 2)
                        {
                            posSize = LotSize1;
                        }
                        else if (rangeHigh - todayGlobexHigh <= atrValue * 3)
                        {
                            posSize = LotSize1 / 2;
                        }
                        else
                        {
                            posSize = (LotSize1 / 3);
                        }
                        posSize = posSize > MaxLotSize ? MaxLotSize : posSize;
                      if (posSize * stopSize * 5 < MaxStop)
                        {
                            int _nr = rnd.Next();
                            string rando = Convert.ToString(_nr);
                            string name = "tag " + rando;
                            _breakoutValid = true;
                            Draw.ArrowUp(this, name, true, 0, Low[0] - 4 * TickSize, Brushes.Blue);
                            _longOneOrder = EnterLong(posSize, "Long Main");
                        }

                    }
                    
                    if (Closes[0][0] <= rangeLow - (TickSize * breakoutTreshold) && !_breakoutValid && noPositions()) 
                    {
                        int _nr = rnd.Next();
                        string rando = Convert.ToString(_nr);
                        string name = "tag " + rando;
                        _breakoutValid = true;
                        Draw.ArrowDown(this, name, true, 0, High[0] + 4 * TickSize, Brushes.Red);
                //        EnterShort("Just short",1);
                    }
                    
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

                else if (BarsInProgress == 2) // 4 min
                {
                    atrDayValue = _atrDay[0];
                }
            }


        }

        private void UpdateLotSizeBasedOnProfit()
        { if (account == null)
            {
             //   Print("Account object is not initialized.");
                return;
            }

            double accountProfit = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
       //     Print("Account Realized Profit/Loss: " + accountProfit);
            int additionalLots = (int)(accountProfit / AmountForSizeUp);
  //          Print(accountProfit);
                int potentialLotSize = LotSize1 + additionalLots;
            LotSize1 = potentialLotSize > MaxLotSize ?  MaxLotSize : potentialLotSize;
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

                SetStopLoss("Long Main",CalculationMode.Price, rangeHigh-1,false);

            } 
             if (status == "Breakeven")
            {
                SetStopLoss("Long Main", CalculationMode.Price, entryPrice, false);

            }
                if (status == "Trail2")
            {

                double stopPrice = Bollinger(2, 10).Lower[0] - atrValue / 2;
                SetStopLoss("Long Main", CalculationMode.Price, stopPrice, false);

                foreach (var order in addonOrders)
                {
                    SetStopLoss(order.Name, CalculationMode.Price, stopPrice, false);
                }
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
            if (OrderFilled(execution.Order) && execution.Order.Name != "Stop loss") //moze to jest problemem
            {
                todayGlobexHigh = Levels4().GetTodayGlobexHigh();
                if (price - rangeHigh > 32 * TickSize)
                {
                    SetStopLoss("Long Main", CalculationMode.Price, rangeHigh -( atrValue / StopPosition), false);
                }
                else
                    SetStopLoss("Long Main", CalculationMode.Price, todayGlobexHigh - ((atrValue * 1.5) / StopLevel), false);

                if (execution.Order == _longOneOrder && execution.Order.OrderAction == OrderAction.Buy)
                {
               //     Print(atrDayValue);
                    SetProfitTarget("Long Main", CalculationMode.Price, price + atrDayValue - (atrValue));
                    _longEntryPrice1 = price;
                    _isLongMainStopped = false;
                    status = "Long Default";
                }

                if (execution.Order.Name == "addon")
                {

                    addonOrders.Add(execution.Order); // Add addon orders to the list
                }
            }
            if (execution.Order.Name == "Long Main" && execution.Order.OrderState == OrderState.Filled && execution.Order.OrderAction ==OrderAction.Sell)
            {
                _isLongMainStopped = true; // Set the flag if "Long Main" is stopped
            }
        }

            private double CalculateStopLoss()
        {
            double stopLoss = 0;

            if (Close[0] - rangeHigh > (32 * TickSize))
            {
                stopLoss = Close[0] - (rangeHigh - (atrValue / StopPosition));
                Print(stopLoss);
          //      stopLoss = StopPosition * TickSize;
            }
            else
            {
                stopLoss = Close[0] - (todayGlobexHigh - (atrValue / StopLevel));
            }

 
            return stopLoss;
        }

        private void AddIndicators()
        {
            _atr = ATR(BarsArray[1], AtrPeriod);
            _atrDay = ATR(BarsArray[2], 14);
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
        public int UserLotSize
        {
            get { return _initialLotSize; }
            set { _initialLotSize = value; }
        }

        [Display(Name = "Use Addon", GroupName = "Position Management", Order = 0)]
        public bool useAddon
        {
            get { return _useAddon; }
            set { _useAddon = value; }
        }

        [Display(Name = "ADD SIZE WITH PROFIT", GroupName = "Position Management", Order = 0)]
        public bool addSize
        {
            get { return _addSize; }
            set { _addSize = value; }
        }


        [Display(Name = "Amount of money for SizeUP", GroupName = "Position Management", Order = 0)]
        public int AmountForSizeUp
        {
            get { return _amountForSizeUp; }
            set { _amountForSizeUp = value; }
        }
        [Display(Name = "Max PositionSize", GroupName = "Filters", Order = 0)]
        public int MaxLotSize
        {
            get { return _maxLotSize; }
            set { _maxLotSize = value; }
        }




        [Display(Name = "Target Base (atr ratio)", GroupName = "Position Management", Order = 0)]
        public int Target1
        {
            get { return _target1; }
            set { _target1 = value; }
        }


        [Display(Name = "Stop  From  Globex (Ticks)", GroupName = "Position Management", Order = 0)]
        public int StopLevel
        {
            get { return _StopLevel; }
            set { _StopLevel = value; }
        }

        [Display(Name = "Stop  From Entry (Ticks)", GroupName = "Position Management", Order = 0)]
        public int StopPosition
        {
            get { return _StopPosition; }
            set { _StopPosition = value; }
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
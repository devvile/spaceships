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
    public class Maximus_KII : Strategy
    {
        #region declarations

        private Indicator _aroon;


        private Order _longOneOrder;
        private Order _shortOneOrder;
        private Indicator _atr;
        private Indicator _stoch;
        private Indicator _aroonSlow;
        private Indicator _aroonMid;
        private Indicator _aroonFast;
        private Indicator _kama;
        private Indicator _ker;

        private double _aroonUp;
        private int _kamaPeriod;
        private int _barsToCheck = 60;
        private double _longStopMargin = 8;
        private double _shortStopMargin = 8;
        private bool _canTrade = false;

        private bool UseRsi = true;
        private double _atrTargetRatio;
        private int _atrPeriod = 100;
        #endregion

        #region My Parameters

        private int _entryFastStochValueLong = 19;
        private int _baseStopMarginLong = 8;
        private double _targetRatioLong = 2.0;

        private int _entryFastStochValueShort = 81;
        private int _baseStopMarginShort = 8;
        private double _targetRatioShort = 2.0;

        private int _aroonPeriod = 24;
        private int _stochRsiPeriod = 9;
        private int _fastMAPeriod = 3;
        private int _slowMAPeriod = 3;
        private int _lookBack = 14;

        private bool _useLongs = true;
        private bool _useShorts = true;
        private bool _useAroon = true;
        private int _aroonFilter = 40;
        private int _aroonPeriodFast = 24;
        private int _aroonPeriodSlow = 24;
        private double atrValue;
        private double stopLossPrice;
        private double _shortEntryPrice1;
        private double _longEntryPrice1;

        private int _lotSize1 = 2;
        private int _lotSize2 = 2;
        private double kerValue;

        private int _rsiEntryShort = 90;
        private int _rsiEntryLong = 10;

        private int BarNr = 0;  //sprawdzic <-----
        Random rnd = new Random();

        private string status = "Flat";


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

        #region STOCH

        [Display(Name = "RSI Period ", GroupName = "STOCH", Order = 0)]
        public int StochRsiPeriod
        {
            get { return _stochRsiPeriod; }
            set { _stochRsiPeriod = value; }
        }

        [Display(Name = "FastMAPeriod", GroupName = "STOCH", Order = 0)]
        public int FastMAPeriod
        {
            get { return _fastMAPeriod; }
            set { _fastMAPeriod = value; }
        }

        [Display(Name = "SlowMAPeriod", GroupName = "STOCH", Order = 0)]
        public int SlowMAPeriod
        {
            get { return _slowMAPeriod; }
            set { _slowMAPeriod = value; }
        }

        [Display(Name = "Look Back", GroupName = "STOCH", Order = 0)]
        public int LookBack
        {
            get { return _lookBack; }
            set { _lookBack = value; }
        }
        #endregion

        #region Longs

        [Display(Name = "Long Stop Margin", GroupName = "LONGS", Order = 0)]
        public double LongStopMargin
        {
            get { return _longStopMargin; }
            set { _longStopMargin = value; }
        }


        #endregion

        #region Shorts

        [Display(Name = "Short Stop Margin", GroupName = "SHORTS", Order = 0)]
        public double ShortStopMargin
        {
            get { return _shortStopMargin; }
            set { _shortStopMargin = value; }
        }

        #endregion

        #region Position Management

        [Display(Name = "Bars to Check", GroupName = "Position Management", Order = 0)]
        public int BarsToCheck
        {
            get { return _barsToCheck; }
            set { _barsToCheck = value; }
        }


        [Display(Name = "Atr Target ratio", GroupName = "Position Management", Order = 0)]
        public double AtrTargetRatio
        {
            get { return _atrTargetRatio; }
            set { _atrTargetRatio = value; }
        }

        #endregion

        #region Filters

        [Display(Name = "Atr period", GroupName = "Filters", Order = 0)]
            public int AtrPeriod
            {
                get { return _atrPeriod; }
                set { _atrPeriod = value; }
            }
        [Display(Name = "Kama period", GroupName = "Filters", Order = 0)]
        public int KamaPeriod
        {
            get { return _kamaPeriod; }
            set { _kamaPeriod = value; }
        }


        #endregion


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Sandbox";
                Name = "Maximus K II";
                Calculate = Calculate.OnBarClose;

            }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;

                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;

            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();
                AddIndicators();
                Calculate = Calculate.OnBarClose;
            }
        }

        protected override void OnBarUpdate()
        {
             if (CurrentBar <= BarsRequiredToTrade) return;

            CalculateTradeTime();
      //      AdjustStop();
            

            if (Position.MarketPosition == MarketPosition.Flat)
            {
                status = "Flat";

            }

            int x = 2;

            if (_canTrade)
            {

                    //Entries
                    atrValue = _atr[0];
                    kerValue = _ker[0];

                    if (_kama[0] - 8 > Close[0] && status != "Long Default" && status != "Short Default" && kerValue <= -0.5 && AroonDown()) //&& Aroon(BarsArray[x], AroonPeriodSlow).Down[0]>70)
                    {
                        Print("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                        Print(Time[0]);
                        Print("Making Short:");
                        _shortOneOrder = EnterShort(1, "Short1");
                        status = "Short Default";
                    }
                    else if (_kama[0] + 8 < Close[0]  && status != "Long Default" && status != "Short Default" && kerValue >= 0.5 && AroonUp())// && Aroon(BarsArray[x], AroonPeriodSlow).Up[0] > 70)
                    {
                        Print("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                        Print(Time[0]);
                        Print("Making Long:");
                        _longOneOrder = EnterLong(1, "Long1");
                        status = "Long Default";
                    }


                    // Exits

                    
                    if (_kama[0] - atrValue * 6 > Close[0] && Position.MarketPosition == MarketPosition.Long)
                    {


                        ExitLong();
                    }
                    else if (_kama[0] + atrValue * 6  < Close[0]  && Position.MarketPosition == MarketPosition.Short)
                    {
 
                        ExitShort();

                    }


                    /*

                    if (CrossAbove(Aroon(BarsArray[x], AroonPeriodSlow).Down, Aroon(BarsArray[x], AroonPeriodSlow).Up, 1) && Position.MarketPosition == MarketPosition.Long)
                    {

                        Print("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                        Print(Time[0]);
                        Print("Closing Long:");
                        Print("Aroon Down");
                        Print(Aroon(BarsArray[x], AroonPeriodSlow).Down[0]);
                        Print("Aroon Up");
                        Print(Aroon(BarsArray[x], AroonPeriodSlow).Up[0]);
                        ExitLong();
                    }
                    else if (CrossAbove(Aroon(BarsArray[x], AroonPeriodSlow).Up, Aroon(BarsArray[x], AroonPeriodSlow).Down, 1) && Position.MarketPosition == MarketPosition.Short)
                    {
                        Print("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
                        Print(Time[0]);
                        Print("Closing Short:");
                        Print("Aroon Down");
                        Print(Aroon(BarsArray[x], AroonPeriodSlow).Down[0]);
                        Print("Aroon Up");
                        Print(Aroon(BarsArray[x], AroonPeriodSlow).Up[0]);
                        ExitShort();

                    }*/
                
            }


        }



        private bool AroonUp()
        {
            return (Aroon(AroonPeriodFast).Up[0] > 10);
        }

        private bool AroonDown()
        {
            return (Aroon(AroonPeriodFast).Down[0] > 10);
        }

 
        private bool previousCandleRed()
        {
            return HeikenAshi8(BarsArray[1]).HAOpen[0] > HeikenAshi8(BarsArray[1]).HAClose[0] && HeikenAshi8(BarsArray[1]).HAOpen[1] < HeikenAshi8(BarsArray[1]).HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8(BarsArray[1]).HAOpen[0] < HeikenAshi8(BarsArray[1]).HAClose[0] && HeikenAshi8(BarsArray[1]).HAOpen[1] > HeikenAshi8(BarsArray[1]).HAClose[1];
        }

        private double calculateStopLong()
        {
            List<double> lows = new List<double> { };
            int i = 0;
            while (i < BarsToCheck)
            {
                lows.Add(Lows[2][i]);

                i++; // increment
            }
            lows.Sort();
            //        double highestHigh = highs[0];

            return lows[0] - LongStopMargin * TickSize;
        }

        private double calculateStopShort()
        {
            List<double> highs = new List<double> { };
            int i = 0;
            while (i < BarsToCheck)
            {
                highs.Add(Highs[2][i]);

                i++; // increment
            }
            highs.Sort();
            highs.Reverse();
            double highestHigh = highs[0];

            return highestHigh + ShortStopMargin * TickSize;
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


        private bool stochRsiEntry(int entryValue, string positionType)
        {
            if (UseRsi)
            {
                double _stochFast = StochRSIMod2NT8(BarsArray[1], StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
                if (positionType == "Long")
                {
                    return _stochFast <= entryValue;
                }
                else if (positionType == "Short")
                {
                    return _stochFast >= entryValue;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

        }

        private bool noPositions()
        {
            return Position.MarketPosition == MarketPosition.Flat;
        }

        private void AdjustStop()
        {
            if (noPositions())
            {
                status = "Flat";
                SetStopLoss(CalculationMode.Ticks, 50);
            }
            else if (status == "Short Default")
            {
                SetStopLoss(CalculationMode.Price, stopLossPrice);
            }
            else if (status == "Long Default")
            {
                SetStopLoss(CalculationMode.Price, stopLossPrice);
            }

        }


        private bool OrderFilled(Order order)
        {
            return order.OrderState == OrderState.Filled;
        }

        private bool IsLongOrder1(Order order)
        {
            return order == _longOneOrder;
        }

        private bool IsShortOrder1(Order order)
        {
            return order == _shortOneOrder;
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (OrderFilled(order) && IsLongOrder1(order))
            {
                _longEntryPrice1 = averageFillPrice;
               // stopLossPrice = calculateStopLong();
                SetStopLoss(CalculationMode.Ticks, 100);
        //        SetStopLoss(CalculationMode.Price, _longEntryPrice1 - atrValue * 0.6);
                SetProfitTarget("Long1", CalculationMode.Price, _longEntryPrice1 + atrValue*AtrTargetRatio);
                status = "Long Default";
            }

            else if (OrderFilled(order) && IsShortOrder1(order))
            {
                _shortEntryPrice1 = averageFillPrice;
           //     stopLossPrice = calculateStopShort();
            SetStopLoss(CalculationMode.Ticks, 100);
             //   SetStopLoss(CalculationMode.Price, _shortEntryPrice1 + atrValue * 0.6);
                SetProfitTarget("Short1", CalculationMode.Price, _shortEntryPrice1 - atrValue * AtrTargetRatio);
                status = "Short Default";
            }
        }

        private void AddIndicators()
        {
            _atr = ATR(AtrPeriod);
            _kama = KAMA(2, KamaPeriod,30);
            _aroonSlow = Aroon(AroonPeriodSlow);
            _ker = EfficiencyRatioIndicator(5, true);
            AddChartIndicator(_atr);
            AddChartIndicator(_aroonSlow);
            AddChartIndicator(_kama);
            AddChartIndicator(_ker);
        }
    }
}
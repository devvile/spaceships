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


        private Order _longOneOrder;
        private Order _shortOneOrder;
        private Indicator _atr;
        private Indicator _stoch;
        private Indicator _aroonSlow;
        private Indicator _aroonMid;
        private Indicator _aroonFast;

        private double _aroonUp;
        private int _barsToCheck;
        private double _longStopMargin;
        private double _shortStopMargin;
        private bool _canTrade = false;

        private bool UseRsi = true;
        private double _atrTargetRatio;
        private int _atrPeriod = 14;
        #endregion

        #region My Parameters

        private int _entryFastStochValueLong;
        private int _baseStopMarginLong;
        private double _targetRatioLong;

        private int _entryFastStochValueShort;
        private int _baseStopMarginShort;
        private double _targetRatioShort;

        private int _aroonPeriod;

        private bool _useLongs = true;
        private bool _useShorts = true;
        private bool _useAroon = true;

        private int _aroonFilter;
        private int _aroonPeriodFast;
        private int _aroonPeriodSlow;
        private double atrValue;
        private double stopLossPrice;
        private double _shortEntryPrice1;
        private double _longEntryPrice1;

        private int _lotSize1;
        private int _lotSize2;


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


        #endregion


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Retest Tracker";
                Name = "Ricochet";
                Calculate = Calculate.OnBarClose;
                BarsRequiredToTrade = 60;
            }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;

                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                BarsRequiredToTrade = BarsToCheck;
                AddDataSeries(BarsPeriodType.Minute, 4);
                AddDataSeries(BarsPeriodType.Minute, 1);
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
            if (CurrentBar < BarsRequiredToTrade || CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade || CurrentBars[2] < BarsRequiredToTrade)
                return;
            CalculateTradeTime();
            //      AdjustStop();


            if (Position.MarketPosition == MarketPosition.Flat)
            {
                status = "Flat";

            }

            int x = 0;

            if (_canTrade)
            {

                if (BarsInProgress == 0)
                {

                    atrValue = _atr[0];

                    //Exits

                }

                else if (BarsInProgress == 1) // 4 min
                {

                    // Exits


                }
                else if (BarsInProgress == 2) // 1 min
                {


                }
            }


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

            /*
            if (OrderFilled(order) && IsLongOrder1(order))
            {
                _longEntryPrice1 = averageFillPrice;
                stopLossPrice = calculateStopLong();
                SetStopLoss(CalculationMode.Ticks, 100);
                //        SetStopLoss(CalculationMode.Price, _longEntryPrice1 - atrValue * 0.6);
                SetProfitTarget("Long1", CalculationMode.Price, _longEntryPrice1 + atrValue * AtrTargetRatio);
                status = "Long Default";
            }

            else if (OrderFilled(order) && IsShortOrder1(order))
            {
                _shortEntryPrice1 = averageFillPrice;
                stopLossPrice = calculateStopShort();
                SetStopLoss(CalculationMode.Ticks, 100);
                //   SetStopLoss(CalculationMode.Price, _shortEntryPrice1 + atrValue * 0.6);
                SetProfitTarget("Short1", CalculationMode.Price, _shortEntryPrice1 - atrValue * AtrTargetRatio);
                status = "Short Default";
            } */
        }

        private void AddIndicators()
        {
            _atr = ATR(BarsArray[0], AtrPeriod);
            _aroonSlow = Aroon(BarsArray[0], AroonPeriodSlow);
            AddChartIndicator(_atr);
            AddChartIndicator(_aroonSlow);
        }
    }
}
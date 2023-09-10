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
    public class StrategyTemplate : Strategy
    {
        #region declarations

        private Indicator _aroon;
        private Indicator _stoch;
        private Indicator _fastHma;
        private Indicator _midHma;
        private Indicator _slowHma;
        private double _aroonUp;
        private int _barsToCheck = 60;
        private double _longStopMargin = 8;
        private double _shortStopMargin = 8;
        private bool _canTrade = false;

        private bool UseRsi = true;

        private int _hmaSlowPeriod = 100;
        private int _hmaMidPeriod = 50;
        private int _hmaFastPeriod = 25;

        private Order _shortOneOrder;
        private Order _shortTwoOrder;
        private Order _shortThreeOrder;

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
        private string _trend;

        private int _lotSize1 = 2;
        private int _lotSize2 = 2;

        private int _rsiEntryShort = 90;
        private int _rsiEntryLong = 10;

        private int BarNr = 0;  //sprawdzic <-----
        Random rnd = new Random();

        private string status = "Flat";


        #endregion

        [Display(Name = "Aroon Period", GroupName = "Aroon", Order = 0)]
        public int AroonPeriod
        {
            get { return _aroonPeriod; }
            set { _aroonPeriod = value; }
        }



        #region HMA

        [Display(Name = "HMA Slow", GroupName = "HMA", Order = 0)]
        public int HmaSlowPeriod
        {
            get { return _hmaSlowPeriod; }
            set { _hmaSlowPeriod = value; }
        }

        [Display(Name = "HMA Mid ", GroupName = "HMA", Order = 0)]
        public int HmaMidPeriod
        {
            get { return _hmaMidPeriod; }
            set { _hmaMidPeriod = value; }
        }

        [Display(Name = "HMA Fast", GroupName = "HMA", Order = 0)]
        public int HmaFastPeriod
        {
            get { return _hmaFastPeriod; }
            set { _hmaFastPeriod = value; }
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

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Long", Order = 0)]
        public int RsiEntryLong
        {
            get { return _rsiEntryLong; }
            set { _rsiEntryLong = value; }
        }
        #endregion

        #region Shorts

        [Display(Name = "Short Stop Margin", GroupName = "SHORTS", Order = 0)]
        public double ShortStopMargin
        {
            get { return _shortStopMargin; }
            set { _shortStopMargin = value; }
        }
        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Short", Order = 0)]
        public int RsiEntryShort
        {
            get { return _rsiEntryShort; }
            set { _rsiEntryShort = value; }
        }
        #endregion

        #region Position Management

        [Display(Name = "Bars to Check", GroupName = "Position Management", Order = 0)]
        public int BarsToCheck
        {
            get { return _barsToCheck; }
            set { _barsToCheck = value; }
        }

        #endregion



        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Sandbox";
                Name = "Strategy Template";
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
            if (CurrentBar < BarsRequiredToTrade || CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade)
                return;
            CalculateTradeTime();

            if (Position.MarketPosition == MarketPosition.Flat)
            {
                status = "Flat";

            }

            if (BarsInProgress == 1) {
                _trend = CalculateTrend();
                if (_canTrade)
                {
                    if(_trend == "Uptrend" && TriggerConditions(_trend) && noPosition())
                    {
                        goLong();
                    }else if(_trend == "Downtrend" && TriggerConditions(_trend) && noPosition())
                    {
                        goShort();
                    }
                    else if (_trend == "Mean Reversion" && TriggerConditions(_trend) && noPosition())
                    {
               //         goBoth();
                    }
                    else
                    {
                        // IGNORE?
                    }
                }
                /*
                if (Position.MarketPosition == MarketPosition.Flat && stochRsiEntry(RsiEntryLong, "Long") && previousCandleGreen() && _canTrade && Aroonize() && HmaFilter())
                {
                    EnterLong(2);
                    SetStopLoss(CalculationMode.Price, calculateStopLong());
                    SetProfitTarget(CalculationMode.Price, calculateStopShort());
                }
                else if (Position.MarketPosition == MarketPosition.Flat && stochRsiEntry(RsiEntryShort, "Short") && previousCandleRed() && _canTrade && Aroonize() && HmaFilter())
                {
                    EnterShort(2);
                    SetStopLoss(CalculationMode.Price, calculateStopShort());
                    SetProfitTarget(CalculationMode.Price, calculateStopLong());
                }*/

            }

        }

        #region Entry

        private void goLong()
        {

        }

        private void goShort()
        {

        }

        #endregion

        private bool noPosition()
        {
            return Position.MarketPosition == MarketPosition.Flat;
        }


        #region Triggers
        private bool TriggerConditions(string trend)
        {
            if(trend == "Uptrend")
            {
                return LongTrigger();
            }
            else if(trend == "Downtrend")
            {
                return ShortTrigger();
            }
            else if(trend == "Mean Reversion")
            {
                return ShortTrigger() || LongTrigger();
            }
            else
            {
                return false;
            }
        }

        private bool LongTrigger()
        {
           return stochRsiEntry(RsiEntryLong, "Long") && previousCandleGreen();
        }

        private bool ShortTrigger()
        {
           return stochRsiEntry(RsiEntryShort, "Short") && previousCandleRed();
        }

        #endregion

        private string CalculateTrend()
        {
            if (IsAroonUp() && IsMALong())
            {
                return "Uptrend";
            }
            else if (IsAroonMeanReversion() && IsMAMeanReversion())
            {
                return "Mean Reversion";
            } else if (IsAroonDown() && IsMAShort())
            {
                return "Downtrend";
            }
            else { return "Not Clear"; }
        
         }

        #region Aroon
        private bool IsAroonUp()
        {
            return (Aroon(BarsArray[0], AroonPeriod).Up[0] > 70);
        }

        private bool IsAroonDown()
        {
            return (Aroon(BarsArray[0], AroonPeriod).Down[0] > 70);
        }

        private bool IsAroonMeanReversion()
        {

            return (Aroon(BarsArray[0], AroonPeriod).Up[0] < 70 && Aroon(BarsArray[0], AroonPeriod).Down[0] < 70);// && (Aroon(BarsArray[0], AroonPeriod).Up[0] > 10 && Aroon(BarsArray[0], AroonPeriod).Down[0] > 10);
        }

        #endregion

        #region MA

        private bool IsMAMeanReversion()
        {

            return (_fastHma[0] > _midHma[0] && _midHma[0] < _slowHma[0]) || (_fastHma[0] < _midHma[0] && _midHma[0] > _slowHma[0]) || (_fastHma[0] < _slowHma[0] && _midHma[0] > _slowHma[0]) || (_fastHma[0] < _slowHma[0] && _midHma[0] > _slowHma[0]);
        }

        private bool IsMALong()
        {
            return (_fastHma[0] > _midHma[0] && _midHma[0] > _slowHma[0]);
        }

        private bool IsMAShort()
        {
            return (_fastHma[0] < _midHma[0] && _midHma[0] < _slowHma[0]);
        }

        #endregion

        #region Ichimoku
        private bool previousCandleRed()
        {
            return HeikenAshi8(BarsArray[1]).HAOpen[0] > HeikenAshi8(BarsArray[1]).HAClose[0] && HeikenAshi8(BarsArray[1]).HAOpen[1] < HeikenAshi8(BarsArray[1]).HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8(BarsArray[1]).HAOpen[0] < HeikenAshi8(BarsArray[1]).HAClose[0] && HeikenAshi8(BarsArray[1]).HAOpen[1] > HeikenAshi8(BarsArray[1]).HAClose[1];
        }
        #endregion


        private double calculateStopLong()
        {
            List<double> lows = new List<double> { };
            int i = 0; 
            while (i < BarsToCheck) 
            {
                lows.Add(Lows[1][i]);

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
                highs.Add(Highs[1][i]);

                i++; // increment
            }
            highs.Sort();
            highs.Reverse();
            double highestHigh = highs[0];

            return highestHigh + ShortStopMargin * TickSize;
        }




        private void CalculateTradeTime()
        {

            if ((ToTime(Time[0]) >= 153000 && ToTime(Time[0]) < 210000))
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
               double  _stochFast = StochRSIMod2NT8(BarsArray[1],StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
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
       
        private void AddIndicators()
        {
            _stoch = StochRSIMod2NT8(BarsArray[1],StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack);
            _aroon = Aroon(BarsArray[0],AroonPeriod);
            _fastHma = HMA(BarsArray[0],HmaFastPeriod);
            _midHma = HMA(BarsArray[0],HmaMidPeriod);
            _slowHma = HMA(BarsArray[0],HmaSlowPeriod);
            AddChartIndicator(_stoch);
            AddChartIndicator(_aroon);
            AddChartIndicator(_fastHma);
            AddChartIndicator(_midHma);
            AddChartIndicator(_slowHma);

        }
    }
}

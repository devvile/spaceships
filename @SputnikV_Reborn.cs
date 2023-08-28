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
    public class Sputnik_V_Reborn : Strategy
    {
        #region Declarations
        private int _positionSize;
        private Indicator _ha;
        private Indicator _aroon;
        private Indicator _stoch;
        Random rnd = new Random();
        private int _aroonPeriod;
        private bool _useAroon;
        private bool _showHA = true;
        private bool _makeTrades = false;
        private double _stochFast;
        #endregion

        #region Parameters

        private int _rsiEntryShort;

        private int _rsiEntryLong;

        private int _stochRsiPeriod;
        private int _fastMAPeriod;
        private int _slowMAPeriod;
        private int _lookBack;
        #endregion


        #region Config

        [Display(Name = "Aroon Period", GroupName = "Config", Order = 0)]
        public int AroonPeriod
        {
            get { return _aroonPeriod; }
            set { _aroonPeriod = value; }
        }

        [Display(Name = "Show HA", GroupName = "Config", Order = 0)]
        public bool ShowHA
        {
            get { return _showHA; }
            set { _showHA = value; }
        }


        [Display(Name = "Use Aroon", GroupName = "Config", Order = 0)]
        public bool UseAroon
        {
            get { return _useAroon; }
            set { _useAroon = value; }
        }

        [Display(Name = "Trade", GroupName = "Config", Order = 0)]
        public bool makeTrades
        {
            get { return _makeTrades; }
            set { _makeTrades = value; }
        }

        #endregion

        #region Position Management

        [Display(Name = "Position Size 1", GroupName = "Position Management", Order = 0)]
        public int PositionSize
        {
            get { return _positionSize; }
            set { _positionSize = value; }
        }
        #endregion

        #region Long

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Long", Order = 0)]
        public int RsiEntryLong
        {
            get { return _rsiEntryLong; }
            set { _rsiEntryLong = value; }
        }

        #endregion

        #region Short

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Short", Order = 0)]
        public int RsiEntryShort
        { 
            get { return _rsiEntryShort; }
            set { _rsiEntryShort = value; }
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


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Sputnik Refacatored";
                Name = "Sputnik V reborn";
                Calculate = Calculate.OnBarClose;
                _positionSize = 2;
                _stochRsiPeriod = 9;
                _fastMAPeriod = 3;
                _slowMAPeriod = 3;
                _lookBack = 14;

                _aroonPeriod = 25;
                _rsiEntryLong = 20;
                _rsiEntryShort = 80;
            }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;
                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                AddHeikenAshi("MES 09-23", BarsPeriodType.Minute, 1, MarketDataType.Last);
            }
            else if (State == State.DataLoaded)
            {
                ClearOutputWindow();
                addIndicators();
                Calculate = Calculate.OnBarClose;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade) return;



            if (LongConditions())
            {
                if (makeTrades)
                {

                }
                else
                {
                    MarkLong();
                }
               // EnterLong(PositionSize);
            }

            else if (ShortConditions())
            {
                if (makeTrades)
                {

                }
                else
                {
                    MarkShort();
                }
                // EnterLong(PositionSize);
            }

        }

        private bool LongConditions()
        {
            return noPositions() && previousCandleGreen() && IsAroonUptrend() && stochRsiEntry(RsiEntryLong ,"Long");
        }

        private bool ShortConditions()
        {
            return noPositions() && previousCandleRed() && IsAroonDowntrend() && stochRsiEntry(RsiEntryShort, "Short");
        }

        private bool previousCandleRed()
        {
            return HeikenAshi8().HAOpen[0] > HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] < HeikenAshi8().HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8().HAOpen[0] < HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] > HeikenAshi8().HAClose[1];
        }

        private void MarkShort()
        {
            int _nr = rnd.Next();
            string rando = Convert.ToString(_nr);
            string name = "tag " + rando;
            Draw.ArrowDown(this, name, true, 0, Low[0] - TickSize, Brushes.Red);
        }

        private void MarkLong()
        {
            int _nr = rnd.Next();
            string rando = Convert.ToString(_nr);
            string name = "tag " + rando;
            Draw.ArrowUp(this, name, true, 0, Low[0] - TickSize, Brushes.Blue);
        }

        private bool IsAroonUptrend()
        {
            if (UseAroon)
            {
                return Aroon(AroonPeriod).Up[0] > Aroon(AroonPeriod).Down[0];
            }
            else
            {
                return true;
            }

        }

        private bool IsAroonDowntrend()
        {
            if (UseAroon)
            {
                return Aroon(AroonPeriod).Up[0] < Aroon(AroonPeriod).Down[0];
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

        private bool stochRsiEntry(int entryValue, string positionType )
        {
            _stochFast = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
            if (positionType == "Long")
            {
               return _stochFast <= entryValue;
            }
            else if( positionType == "Short")
            {
              return  _stochFast >= entryValue;
            }
            else
            {
                return false;
            }

        }

        private void addIndicators()
        {
            _ha = HeikenAshi8();
            _aroon = Aroon( AroonPeriod);
            _stoch = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack);
            AddChartIndicator(_aroon);
            if (ShowHA)
            {
                AddChartIndicator(_ha);
            }
            AddChartIndicator(_stoch);

     //       AddChartIndicator(IchimokuCloud());
        }
    }
}

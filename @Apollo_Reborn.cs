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
    public class ApolloReborn : Strategy
    {
        #region declarations

        private int _positionSize;
        private Indicator _ha;
        private Indicator _aroon;
        private Indicator _stoch;
        Random rnd = new Random();
        private int _aroonPeriod;
        private bool _useAroon;
        private bool _showHA = true;
        private bool _useRsi = false;
        private double _stochFast;

        private int _rsiEntryShort;

        private int _rsiEntryLong;

        private int _stochRsiPeriod;
        private int _fastMAPeriod;
        private int _slowMAPeriod;
        private int _lookBack;



        #endregion





        #region Position Management

        [Display(Name = "Position Size 1", GroupName = "Position Management", Order = 0)]
        public int PositionSize
        {
            get { return _positionSize; }
            set { _positionSize = value; }
        }


        [Display(Name = "Aroon Period", GroupName = "Position Management", Order = 0)]
        public int AroonPeriod
        {
            get { return _aroonPeriod; }
            set { _aroonPeriod = value; }
        }

        [Display(Name = "Show HA", GroupName = "Position Management", Order = 0)]
        public bool ShowHA
        {
            get { return _showHA; }
            set { _showHA = value; }
        }



        [Display(Name = "Use Aroon", GroupName = "Position Management", Order = 0)]
        public bool UseAroon
        {
            get { return _useAroon; }
            set { _useAroon = value; }
        }

        [Display(Name = "Use RSI", GroupName = "Config", Order = 0)]
        public bool UseRsi
        {
            get { return _useRsi; }
            set { _useRsi = value; }
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

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Long", Order = 0)]
        public int RsiEntryLong
        {
            get { return _rsiEntryLong; }
            set { _rsiEntryLong = value; }
        }

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Short", Order = 0)]
        public int RsiEntryShort
        {
            get { return _rsiEntryShort; }
            set { _rsiEntryShort = value; }
        }


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Longer";
                Name = "Apollo Reborn";
                Calculate = Calculate.OnBarClose;
                _positionSize = 2;
                _stochRsiPeriod = 9;
                _fastMAPeriod = 3;
                _slowMAPeriod = 3;
                _lookBack = 14;
            }

            else if (State == State.Configure)
            {
                ClearOutputWindow();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
                Calculate = Calculate.OnBarClose;

                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                //    AddHeikenAshi("MNQ 09-23", BarsPeriodType.Minute, 1, MarketDataType.Last);
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
                Mark("Long");
               // EnterLong(PositionSize);
            }

            else if (ShortConditions())
            {
                Mark("Short");
               // EnterShort(PositionSize);
            }

        }

        private void Mark(string positionType)
        {
            int _nr = rnd.Next();
            string rando = Convert.ToString(_nr);
            string name = "tag " + rando;
            if (positionType == "Short")
            {
                Draw.ArrowDown(this, name, true, 0, High[0] + TickSize, Brushes.Red);
            }
            else if (positionType == "Extra Short")
            {
                Draw.ArrowDown(this, name, true, 0, High[0] + TickSize, Brushes.Yellow);
            }
            else if (positionType == "Long")
            {
                Draw.ArrowUp(this, name, true, 0, Low[0] - TickSize, Brushes.Blue);
            }
            else if (positionType == "Extra Long")
            {
                Draw.ArrowUp(this, name, true, 0, Low[0] - TickSize, Brushes.Yellow);
            }

        }

        private bool LongConditions()
        {
            return previousCandleGreen() && IsAroonUptrend() && stochRsiEntry(RsiEntryLong, "Long");
        }

        private bool ShortConditions()
        {
            return previousCandleRed() && IsAroonDowntrend() && stochRsiEntry(RsiEntryShort, "Short");
        }

        private bool previousCandleRed()
        {
            return HeikenAshi8().HAOpen[0] > HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] < HeikenAshi8().HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8().HAOpen[0] < HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] > HeikenAshi8().HAClose[1];
        }

        private bool stochRsiEntry(int entryValue, string positionType)
        {
            if (UseRsi)
            {
                _stochFast = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack).SK[1];
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

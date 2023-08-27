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

        #endregion




        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Longer";
                Name = "Apollo Reborn";
                Calculate = Calculate.OnBarClose;
                _positionSize = 2;
                

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
                MarkLong();
               // EnterLong(PositionSize);
            }

            else if (ShortConditions())
            {
                MarkShort();
               // EnterShort(PositionSize);
            }

        }

        private bool LongConditions()
        {
            return previousCandleGreen() && IsAroonUptrend();
        }

        private bool ShortConditions()
        {
            return previousCandleRed() && IsAroonDowntrend();
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

        private void addIndicators()
        {
            _ha = HeikenAshi8();

            _aroon = Aroon( AroonPeriod);
            _stoch = StochRSIMod2NT8(9, 3, 3, 14);
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

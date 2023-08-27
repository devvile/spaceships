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
    public class Longer : Strategy
    {
        #region declarations

        private int _positionSize;
        private Indicator _ha;
        Random rnd = new Random();

        #endregion





        #region Position Management

        [Display(Name = "Position Size 1", GroupName = "Position Management", Order = 0)]
        public int PositionSize
        {
            get { return _positionSize; }
            set { _positionSize = value; }
        }

        #endregion




        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Longer";
                Name = "Longer indciator HA";
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



            if (Bars.IsFirstBarOfSession)
            {
                EnterLong(PositionSize);
            }

            if (previousCandleRed())
            {
                int _nr  = rnd.Next();
                string rando = Convert.ToString(_nr);
                string name = "tag " + rando;
                Draw.ArrowUp(this, name, true, 0, Low[0] - TickSize, Brushes.Red);
            }

            if (previousCandleRed())
            {
                int _nr = rnd.Next();
                string rando = Convert.ToString(_nr);
                string name = "tag " + rando;
                Draw.ArrowUp(this, name, true, 0, Low[0] - TickSize, Brushes.Red);
            }

        }


        private bool previousCandleRed()
        {
            return HeikenAshi8().HAOpen[0] > HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] < HeikenAshi8().HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8().HAOpen[0] < HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] > HeikenAshi8().HAClose[1];
        }

        private void addIndicators()
        {
            _ha = HeikenAshi8();
            AddChartIndicator(_ha);

        }
    }
}

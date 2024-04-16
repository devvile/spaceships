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
    public class reVolt : Strategy
    {
        #region declarations


        private int _period = 10;
        private Series<double> meanVolume;
        private string status = "Flat";


        #endregion





        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Sandbox";
                Name = "ReVolt";
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
            }
            else if (State == State.DataLoaded)
            {
                meanVolume = new Series<double>(this);
                ClearOutputWindow();
                Calculate = Calculate.OnBarClose;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
                return;

            double totalVolume = 0;
            Print("xxxxx");
            Print("Current Bar:");

            Print(Time[0]);
            Print("Vol:");
            Print(Volumes[0][0]);
            Print("``````");
            int daysToCheck = _period;
            int i = 1;
            while (i <= daysToCheck) 
            {
                i++;
                //   CurrentBars[0] -  Bars.GetBar(Time[0]));

                TimeSpan ts = new TimeSpan(Time[0].Hour, Time[0].Minute, Time[0].Second);
                DateTime dateToCheck =  new DateTime(Time[0].Year, Time[0].Month, Time[0].Day);
                dateToCheck =dateToCheck.Date.AddDays(-i);
                if(dateToCheck.DayOfWeek != DayOfWeek.Sunday && dateToCheck.DayOfWeek != DayOfWeek.Saturday)
                {
                    dateToCheck = dateToCheck.Date + ts;
                    int barsAgo = CurrentBars[0] - Bars.GetBar(dateToCheck);
                    /*
                    Print("xxxxxxxxxxx");
                    Print(dateToCheck);
                    Print(dateToCheck.DayOfWeek);
                    Print("Bars Ago:");
                    Print(barsAgo);
                    Print("Price:");
                    Print(Closes[0][barsAgo]);
                    Print("Volume:");
                    Print(Volumes[0][barsAgo]);
                    */
                    totalVolume += Volumes[0][barsAgo];
                }
                else
                {
                    daysToCheck++;
                }

            }
            Print("vvvvvvvvvvvvvvvvvvvvvvv");
            Print("Total Volume");
            Print(totalVolume);
            Print("Mean Volume");
            Print(totalVolume / _period);
            meanVolume[0] = totalVolume / _period;
            double rvol = Volume[0] / meanVolume[0];
            Print("Revol");
            Print(rvol);
            //       Values[0][0] = rvol;

        }
    }
}
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
    public class Maximus_Resurrected : Strategy
    {
        #region declarations

        private Indicator _aroon;
        private Indicator _momentum1;
        private Indicator _momentum2;


        private double _momentum1TriggerValue;
        private double _momentum2TriggerValue;
        private bool _canTrade;

        int _rthStartTime;
        int _rthEndTime;
        int _IbEndTime;
        Random rnd = new Random();
        #endregion


        #region My Parameters

        #region Momentum

        [Display(Name = "Momentum 1 Trigger (EMA)", GroupName = "Aroon", Order = 0)]
        public double Momentum1
        {
            get { return _momentum1TriggerValue; }
            set { _momentum1TriggerValue = value; }
        }


        [Display(Name = "Momentum 2 Trigger (BB)", GroupName = "Aroon", Order = 0)]
        public double Momentum2TriggerValue
        {
            get { return _momentum2TriggerValue; }
            set { _momentum2TriggerValue = value; }
        }


        #endregion



        #endregion




        // Momentum trigger
        // Momentum trigger 2

        // aroon Uptrend > 70 i donw ponizej 70
        // aroon downtrend < 70 i up pow 70

        // pullback / candle trigger (jesli zmienia kolor na czerwony


        protected override void OnStateChange()
        {

            if (State == State.SetDefaults)
            {
                Description = @"Sandbox";
                Name = "Maximus Resurrected";
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
            if (CurrentBar < BarsRequiredToTrade)
                return;

  
            int intDate = ToDay(Time[0]); // Get integer representation of the date
            bool isSpecialPeriod = DateRanges.Any(range => range.Contains(intDate));
            _rthStartTime = isSpecialPeriod ? 143000 : 153000;
            _rthEndTime = isSpecialPeriod ? 210000 : 220000;
            _IbEndTime = isSpecialPeriod ? 153000 : 163000;

            CalculateTradeTime();
            if (_canTrade)
            {
                if (AroonUp() && lastCandleRed() && _momentum1[0] >= _momentum1TriggerValue && _momentum2[0] >= _momentum2TriggerValue)
                {
                    int _nr = rnd.Next();
                    string rando = Convert.ToString(_nr);
                    string name = "tag " + rando;
                    Draw.ArrowUp(this, name, true, 0, Low[0] - 4 * TickSize, Brushes.Blue);
                }
                 if (AroonDown()&& lastCandleGreen() && _momentum1[0] <= _momentum1TriggerValue * -1 && _momentum2[0] <= _momentum2TriggerValue * -1)
                {
                    int _nr = rnd.Next();
                    string rando = Convert.ToString(_nr);
                    string name = "tag " + rando;
                    Draw.ArrowDown(this, name, true, 0, High[0] + 4 * TickSize, Brushes.Red);
                }
            }

        }



        private bool AroonUp()
        {
            return (Aroon(15).Up[0] > 70 && Aroon(15).Down[0] < 30);
        }

        private bool AroonDown()
        {
            return (Aroon(15).Down[0] > 70 && Aroon(15).Up[0] < 30);
        }

 
        private bool lastCandleRed()
        {
            bool isCurrentRed = Close[0] < Open[0];
            bool isPreviousGreen = Close[1] > Open[1];

            return isCurrentRed && isPreviousGreen;
        }

        private bool lastCandleGreen()
        {
            bool isCurrentGreen = Close[0] > Open[0];
            bool isPreviousRed = Close[1] < Open[1];

            return isCurrentGreen && isPreviousRed;
        }



        private void CalculateTradeTime()
        {

            if ((ToTime(Time[0]) >= _rthStartTime && ToTime(Time[0]) < _rthEndTime))
            {
                _canTrade = true;
            }
            else
            {
                _canTrade = false;
            }
        }


        private void AddIndicators()
        {
            _aroon = Aroon(15);
            _momentum1 = Momentum(EMA(10),5);
            _momentum2 = Momentum(Bollinger(2,14).Middle, 5);
            AddChartIndicator(_aroon);
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

    }
}
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
        private int _rthStartTime;
        private int _rthEndTime;
        private int _ibEndTime;
        private Random _rnd = new Random();
        private List<DateRange> _dateRanges;
        private bool _signalModeActive;
        private CandleColor _signalModeColor;
        private int _aroonLowTreshold;
        private int _aroonHighTreshold;
        private double entryPrice;
        #endregion


        #region My Parameters

        #region Momentum
        [Display(Name = "Momentum 1 Trigger (EMA)", GroupName = "Momentum", Order = 0)]
        public double Momentum1TriggerValue
        {
            get { return _momentum1TriggerValue; }
            set {_momentum1TriggerValue = value;}
        }

        [Display(Name = "Momentum 2 Trigger (BB)", GroupName = "Momentum", Order = 1)]
        public double Momentum2TriggerValue
        {
            get  {return   _momentum2TriggerValue; }
            set  { _momentum2TriggerValue = value; }
        }


        #endregion

        #region Aroon
        [Display(Name = "Aroon Low Treshold", GroupName = "Momentum", Order = 0)]
        public int AroonLowTreshold
        {
            get { return _aroonLowTreshold; }
            set { _aroonLowTreshold = value; }
        }

        [Display(Name = "Aroon High Treshold", GroupName = "Momentum", Order = 1)]
        public int AroonHighTreshold
        {
            get { return _aroonHighTreshold; }
            set { _aroonHighTreshold = value; }
        }

        #endregion

        #endregion


        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Maximus Resurrected Strategy";
                Name = "Maximus Resurrected";
                Calculate = Calculate.OnBarClose;
                BarsRequiredToTrade = 60;
            }
            else if (State == State.Configure)
            {
                ConfigureDateRanges();
                EntryHandling = EntryHandling.AllEntries;
                EntriesPerDirection = 6;
            }
            else if (State == State.DataLoaded)
            {
                AddIndicators();
            }
        }

        private void ConfigureDateRanges()
        {
            _dateRanges = new List<DateRange>
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
        //ekspansja range'u
        // dodanie strzalki entry 
        //Jesli low swieczki zamyka sie pow/pon low/high siweczki sygnalowej i jest na niej conajmniej 2/3 warunkow trybu active to mamy entry
        // dodanie punktu stop
        //zebrac informacje o ostatniej swieczce active


        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade) return;
            CheckTradingWindow();
            if (_canTrade)
            {
                CandleColor currentCandleColor = DetermineCandleColor(0);
                CandleColor previousCandleColor = DetermineCandleColor(1);

                if ((previousCandleColor == CandleColor.Doji && currentCandleColor!=CandleColor.Doji) || (LastCandleColorChange(currentCandleColor) || currentCandleColor == CandleColor.Doji)) {
                    _signalModeActive = true;
                    _signalModeColor = currentCandleColor;
                    entryPrice = setEntryPrice(currentCandleColor);
                }

                else if (currentCandleColor == _signalModeColor)
                {
                    _signalModeActive = true;
                    entryPrice = setEntryPrice(currentCandleColor);
                }
                else
                {
                    _signalModeActive = false;
                }

                if (_signalModeActive == true)
                {
                    ProcessSignalConditions(currentCandleColor, _signalModeColor, entryPrice);
                }
                else
                {
                    ProcessEntryConditions(currentCandleColor, _signalModeColor, entryPrice);
                }
            }
        }

        private void ProcessEntryConditions(CandleColor currentCandleColor, CandleColor _signalModeColor, double entryPrice)
        {
            int conditionsMetLong = CheckLongConditions();
            int conditionsMetShort = CheckShortConditions();

            if(currentCandleColor != _signalModeColor && currentCandleColor == CandleColor.Red && Low[0] < entryPrice && conditionsMetShort >= 2)
            {
                DrawSignal("Short", High[0] + 4 * TickSize, Brushes.Yellow);
            }/*
            if (currentCandleColor != _signalModeColor && currentCandleColor == CandleColor.Red && Low[0] < entryPrice && conditionsMetShort >= 2)
            {
                DrawSignal("Short", High[0] + 4 * TickSize, Brushes.Yellow);
            }
            */
        }

        private void ProcessSignalConditions(CandleColor currentCandleColor, CandleColor _signalModeColor, double entryPrice)
        {
            int conditionsMetLong = CheckLongConditions();
            int conditionsMetShort = CheckShortConditions();

            ExecuteSignals(conditionsMetLong, conditionsMetShort, currentCandleColor);
        }

        private bool LastCandleColorChange(CandleColor targetColor)
        {
            bool isCurrentCandleTargetColor = (targetColor == CandleColor.Green) ? Close[0] > Open[0] : Close[0] < Open[0];
            bool isPreviousCandleOppositeColor = (targetColor == CandleColor.Green) ? Close[1] < Open[1] : Close[1] > Open[1];

            return isCurrentCandleTargetColor && isPreviousCandleOppositeColor;
        }

        private void ExecuteSignals(int conditionsMetLong, int conditionsMetShort, CandleColor currentCandleColor)
        {
            if ((currentCandleColor == CandleColor.Red || currentCandleColor == CandleColor.Doji) && conditionsMetLong >= 2)
            {
                DrawSignal("Long", Low[0] - 4 * TickSize, Brushes.Blue);
            }
            if ((currentCandleColor == CandleColor.Green || currentCandleColor == CandleColor.Doji) && conditionsMetShort >= 2)
            {
                DrawSignal("Short", High[0] + 4 * TickSize, Brushes.Red);
            }
        }

        private double setEntryPrice(CandleColor currentCandleColor)
        {
            if (currentCandleColor== CandleColor.Green) {
                return Low[0];
            }
            else
            {
                return High[0];
            }
        }

        private CandleColor DetermineCandleColor(int index)
        {
            if (Close[index] > Open[index])
            {
                return CandleColor.Green;
            }
            if (Close[index] == Open[index])
            {
                return CandleColor.Doji;
            }
            return CandleColor.Red;
        }

        private void CheckTradingWindow()
        {
            int intDate = ToDay(Time[0]);
            bool isSpecialPeriod = _dateRanges.Any(range => range.Contains(intDate));
            _rthStartTime = isSpecialPeriod ? 143000 : 153000;
            _rthEndTime = isSpecialPeriod ? 210000 : 220000;
            _ibEndTime = isSpecialPeriod ? 153000 : 163000;

            int currentTime = ToTime(Time[0]);
            _canTrade = currentTime >= _rthStartTime && currentTime < _rthEndTime;
        }


        private int CheckLongConditions()
        {
            bool condition1 = Aroon(15).Up[0] > AroonHighTreshold && Aroon(15).Down[0] < AroonLowTreshold;
            bool condition2 = _momentum1[0] >= _momentum1TriggerValue;
            bool condition3 = _momentum2[0] >= _momentum2TriggerValue;
            return (condition1 ? 1 : 0) + (condition2 ? 1 : 0) + (condition3 ? 1 : 0);
        }

        private int CheckShortConditions()
        {
            bool condition1 = Aroon(15).Down[0] > AroonHighTreshold && Aroon(15).Up[0] < AroonLowTreshold;
            bool condition2 = _momentum1[0] <= -_momentum1TriggerValue;
            bool condition3 = _momentum2[0] <= -_momentum2TriggerValue;
            return (condition1 ? 1 : 0) + (condition2 ? 1 : 0) + (condition3 ? 1 : 0);
        }

        private enum CandleColor
        {
            Red,
            Green,
            Doji
        }



        private void DrawSignal(string direction, double price, Brush color)
        {
            int randomNumber = _rnd.Next();
            string tag = "tag" + randomNumber;
            if (direction == "Long")
            {
                Draw.ArrowUp(this, tag, true, 0, price, color);
            //    Draw.Diamond(this, tag + "dot", true, 0, Lows[0][0] - 4, Brushes.Yellow);
            }
            else
            {
                Draw.ArrowDown(this, tag, true, 0, price, color);
            }
        }


        private void AddIndicators()
        {
            _aroon = Aroon(15);
            _momentum1 = Momentum(EMA(10),5);
            _momentum2 = Momentum(Bollinger(2,14).Middle, 5);
        //    AddChartIndicator(_aroon);
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
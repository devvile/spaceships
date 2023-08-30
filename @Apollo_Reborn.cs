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
    public class Apollo_REBORN : Strategy
    {
        #region Declarations
        private int _lotSize1;
        private int _lotSize2;
        private int _lotSize3;
        private Indicator _ha;
        private Indicator _aroon;
        private Indicator _stoch;

        Random rnd = new Random();
        private int _aroonPeriod;
        private bool _useAroon;
        private bool _showHA = true;
        private bool _useRsi = false;
        private bool _makeTrades = false;
        private bool _useLongs = true;
        private bool _useShorts = true;
        private double _stochFast;

        private double _longEntryPrice1;
        private double _stopLossBaseLong;
        private int _profitTargetLong1;
        private int _profitTargetLong2;
        private int _profitTargetLong3;
        private Order _longOneOrder;
        private Order _longTwoOrder;
        private Order _longThreeOrder;
        private int _baseStopMarginLong;


        private double _shortEntryPrice1;
        private double _stopLossBaseShort;
        private int _profitTargetShort1;
        private int _profitTargetShort2;
        private int _profitTargetShort3;
        private Order _shortOneOrder;
        private Order _shortTwoOrder;
        private Order _shortThreeOrder;
        private int _baseStopMarginShort;

        private int _extraSize;
        #endregion

        #region Parameters

        private int _rsiEntryShort;

        private int _rsiEntryLong;

        private int _stochRsiPeriod;
        private int _fastMAPeriod;
        private int _slowMAPeriod;
        private int _lookBack;
        private string status = "Flat";
        private bool _canTrade = false;

        //   private int _minuteIndex =0 ;
        #endregion


        #region Config

        [Display(Name = "Aroon Period", GroupName = "Config", Order = 4)]
        public int AroonPeriod
        {
            get { return _aroonPeriod; }
            set { _aroonPeriod = value; }
        }

        [Display(Name = "Show HA", GroupName = "Config", Order = 1)]
        public bool ShowHA
        {
            get { return _showHA; }
            set { _showHA = value; }
        }


        [Display(Name = "Use Aroon", GroupName = "Config", Order = 3)]
        public bool UseAroon
        {
            get { return _useAroon; }
            set { _useAroon = value; }
        }

        [Display(Name = "Use RSI", GroupName = "Config", Order = 2)]
        public bool UseRsi
        {
            get { return _useRsi; }
            set { _useRsi = value; }
        }

        [Display(Name = "Trade", GroupName = "Config", Order = 0)]
        public bool makeTrades
        {
            get { return _makeTrades; }
            set { _makeTrades = value; }
        }


        #endregion

        #region Position Management

        [Display(Name = "Entry Size 1", GroupName = "Position Management", Order = 0)]
        public int LotSize1
        {
            get { return _lotSize1; }
            set { _lotSize1 = value; }
        }

        [Display(Name = "Entry Size 2", GroupName = "Position Management", Order = 0)]
        public int LotSize2
        {
            get { return _lotSize2; }
            set { _lotSize2 = value; }
        }

        [Display(Name = "Entry Size 3", GroupName = "Position Management", Order = 0)]
        public int LotSize3
        {
            get { return _lotSize3; }
            set { _lotSize3 = value; }
        }

        #endregion

        #region Long

        [Display(Name = "Use Longs", GroupName = "Long", Order = 0)]
        public bool UseLongs
        {
            get { return _useLongs; }
            set { _useLongs = value; }
        }

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Long", Order = 0)]
        public int RsiEntryLong
        {
            get { return _rsiEntryLong; }
            set { _rsiEntryLong = value; }
        }

        [Display(Name = "Profit Target 1", GroupName = "Long", Order = 0)]
        public int ProfitTargetLong1
        {
            get { return _profitTargetLong1; }
            set { _profitTargetLong1 = value; }
        }

        [Display(Name = "Profit Target 2", GroupName = "Long", Order = 0)]
        public int ProfitTargetLong2
        {
            get { return _profitTargetLong2; }
            set { _profitTargetLong2 = value; }
        }

        [Display(Name = "Profit Target 3", GroupName = "Long", Order = 0)]
        public int ProfitTargetLong3
        {
            get { return _profitTargetLong3; }
            set { _profitTargetLong3 = value; }
        }

        [Display(Name = "Base Stop Margin", GroupName = "Long", Order = 0)]
        public int BaseStopMarginLong
        {
            get { return _baseStopMarginLong; }
            set { _baseStopMarginLong = value; }
        }

        #endregion

        #region Short

        [Display(Name = "Use Shorts", GroupName = "Short", Order = 0)]
        public bool UseShorts
        {
            get { return _useShorts; }
            set { _useShorts = value; }
        }

        [Display(Name = "Stoch Rsi Entry Value", GroupName = "Short", Order = 0)]
        public int RsiEntryShort
        {
            get { return _rsiEntryShort; }
            set { _rsiEntryShort = value; }
        }
        [Display(Name = "Profit Target 1", GroupName = "Short", Order = 0)]
        public int ProfitTargetShort1
        {
            get { return _profitTargetShort1; }
            set { _profitTargetShort1 = value; }
        }

        [Display(Name = "Profit Target 2", GroupName = "Short", Order = 0)]
        public int ProfitTargetShort2
        {
            get { return _profitTargetShort2; }
            set { _profitTargetShort2 = value; }
        }

        [Display(Name = "Profit Target 3", GroupName = "Short", Order = 0)]
        public int ProfitTargetShort3
        {
            get { return _profitTargetShort3; }
            set { _profitTargetShort3 = value; }
        }

        [Display(Name = "Base Stop Margin", GroupName = "Short", Order = 0)]
        public int BaseStopMarginShort
        {
            get { return _baseStopMarginShort; }
            set { _baseStopMarginShort = value; }
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
                Name = "Apollo Reborn";
                Calculate = Calculate.OnBarClose;
                _lotSize1 = 1;
                _lotSize2 = 1;
                _lotSize3 = 1;
                _profitTargetLong1 = 12;
                _profitTargetLong2 = 12;
                _profitTargetLong3 = 12;
                _baseStopMarginLong = 18;
                _baseStopMarginShort = 18;
                _profitTargetShort1 = 12;
                _profitTargetShort2 = 12;
                _profitTargetShort3 = 12;
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
            if (CurrentBar <= BarsRequiredToTrade) return;

            CalculateTradeTime();

            if (_canTrade)
            {
                TradeLikeAKing();

            }
        }
        private void TradeLikeAKing()
        {
            if (UseLongs)
            {
                if (LongConditions())
                {
                    if (makeTrades)
                    {
                        _longOneOrder = EnterLong(LotSize1, "Long1");
                        _longTwoOrder = EnterLong(LotSize2, "Long2");
                        _longThreeOrder = EnterLong(LotSize3, "Long3");
                    }
                    else
                    {
                        Mark("Long");
                    }
                }
            }

            if (UseShorts)
            {
                if (ShortConditions())
                {
                    if (makeTrades)
                    {
                        _shortOneOrder = EnterShort(LotSize1, "Short1");
                        _shortTwoOrder = EnterShort(LotSize2, "Short2");
                        _shortThreeOrder = EnterShort(LotSize3, "Short3");
                    }
                    else
                    {
                        Mark("Short");
                    }
                }
            }

        }


        #region Entry Positions

        private bool LongConditions()
        {
            return noPositions() && previousCandleGreen() && IsAroonUptrend() && stochRsiEntry(RsiEntryLong, "Long"); 
        }


        private bool ShortConditions()
        {
            return noPositions() && previousCandleRed() && IsAroonDowntrend() && stochRsiEntry(RsiEntryShort, "Short");
        }


        #endregion  

        private bool previousCandleRed()
        {
            return HeikenAshi8().HAOpen[0] > HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] < HeikenAshi8().HAClose[1];
        }

        private bool previousCandleGreen()
        {
            return HeikenAshi8().HAOpen[0] < HeikenAshi8().HAClose[0] && HeikenAshi8().HAOpen[1] > HeikenAshi8().HAClose[1];
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

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
OrderState orderState, DateTime time, ErrorCode error, string comment)
        {
            if (OrderFilled(order) && IsLongOrder1(order))
            {
                _longEntryPrice1 = averageFillPrice;
                _stopLossBaseLong = calculateStopLong();
                SetStopLoss("Basic Long Entry1", CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Basic Long Entry1", CalculationMode.Ticks, ProfitTargetLong1);
                status = "Long 1 Default";
            }
            else if (OrderFilled(order) && IsLongOrder2(order))
            {
                SetStopLoss("Basic Long Entry2", CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Basic Long Entry2", CalculationMode.Ticks, ProfitTargetLong2);
            }
            else if (OrderFilled(order) && IsLongOrder3(order))
            {
                SetStopLoss("Basic Long Entry3", CalculationMode.Price, _stopLossBaseLong, false);
                SetProfitTarget("Basic Long Entry3", CalculationMode.Ticks, ProfitTargetLong3);
            }

            else if (OrderFilled(order) && IsShortOrder1(order))
            {
                _shortEntryPrice1 = averageFillPrice;
                _stopLossBaseShort = calculateStopShort();
                SetStopLoss("Basic Short Entry1", CalculationMode.Price, _stopLossBaseShort, false);
                SetProfitTarget("Basic Short Entry1", CalculationMode.Ticks, ProfitTargetShort1);
                status = "Short 1 Default";
            }
            else if (OrderFilled(order) && IsShortOrder2(order))
            {
                SetStopLoss("Basic Short Entry2", CalculationMode.Price, _stopLossBaseShort, false);
                SetProfitTarget("Basic Short Entry2", CalculationMode.Ticks, ProfitTargetShort2);
            }
            else if (OrderFilled(order) && IsShortOrder3(order))
            {
                SetStopLoss("Basic Short Entry3", CalculationMode.Price, _stopLossBaseShort, false);
                SetProfitTarget("Basic Short Entry3", CalculationMode.Ticks, ProfitTargetShort3);
            }

        }

        #region tradeTime
        private void CalculateTradeTime()
        {

            if ((ToTime(Time[0]) >= 152900 && ToTime(Time[0]) < 215000))
            {
                _canTrade = true;
            }
            else
            {
                _canTrade = false;
            }
        }

        #endregion

        #region Orders Conditions

        private bool IsLongOrder1(Order order)
        {
            return order == _longOneOrder;
        }

        private bool IsLongOrder2(Order order)
        {
            return order == _longTwoOrder;
        }

        private bool IsLongOrder3(Order order)
        {
            return order == _longThreeOrder;
        }


        private bool IsShortOrder1(Order order)
        {
            return order == _shortOneOrder;
        }

        private bool IsShortOrder2(Order order)
        {
            return order == _shortTwoOrder;
        }

        private bool IsShortOrder3(Order order)
        {
            return order == _shortThreeOrder;
        }


        private bool OrderFilled(Order order)
        {
            return order.OrderState == OrderState.Filled;
        }

        #endregion

        #region Stop Calculation
        private double calculateStopLong()
        {
            List<double> lows = new List<double> { Low[0], Low[1], Low[2], Low[3], Low[4] };
            lows.Sort();
            double lowestLow = lows[0];
            double baseStopLoss = lowestLow - BaseStopMarginLong * TickSize;

            return baseStopLoss;
        }

        private double calculateStopShort()
        {
            List<double> highs = new List<double> { High[0], High[1], High[2], High[3], High[4] };
            highs.Sort();
            highs.Reverse();
            double highestHigh = highs[0];
            double baseStopLoss = highestHigh + BaseStopMarginShort * TickSize;

            return baseStopLoss;
        }
        #endregion

        private void addIndicators()
        {
            _ha = HeikenAshi8();
            _aroon = Aroon(AroonPeriod);
            _stoch = StochRSIMod2NT8(StochRsiPeriod, FastMAPeriod, SlowMAPeriod, LookBack);

            if (ShowHA)
            {
                AddChartIndicator(_ha);
            }

            if (UseRsi)
            {
                AddChartIndicator(_stoch);
            }

            AddChartIndicator(_aroon);

            //       AddChartIndicator(IchimokuCloud());
        }
    }
}
